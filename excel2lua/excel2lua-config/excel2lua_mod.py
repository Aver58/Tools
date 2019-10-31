#! /usr/bin/env python
# coding=utf-8
'''
#========================================================================
#   FileName: excel2lua_mod.py
#     Author: kevinlin
#   History:

2013年8月15日 18:10:58  创建文件

2013年8月15日 22:27:06 添加了对默认值的支持。当为空时，使用默认值。

2013年8月16日 11:50:21 添加带级别的标准输出？ 和打印日志类似。如设置级别高时，只打印特别重要的日志，当级别低时，一些调试的日志会打印出来 当脚本解析出问题时，调整日志级别以便更好的定位问题

2013年8月21日 22:21:20 将通过这个导出的配置统一在模块config下。其实不用模块更好 。直接全局可见？

2013年8月28日 12:48:32 添加对表格中有重复的key的解析。在使用脚本时通过参数指定是否有此行为

2013年9月13日 16:13:15 将这个解析模块独立出来。main拆在别的文件中实现

2013年9月5日 19:37:25  添加支持subkey.当key有重复时，可以指定是否有sub_key, 如果设置sub_key_col为非0.则认为那行是sub key的值。

2013年11月8日 13:29:58 将table中的索引从1开始（之前从0，不太符合lua的数组规则)

2013年11月14日 16:21:36 支持表格的key为string。生成的table形式如 ["宋江"] = {70000100, 70000101}

2013年12月2日 17:10:15 对于array数组解析，自动去除多余的分隔符（在文本末尾的分隔符,如123;355;这里的最后那个分号;）

2013年12月19日 17:07:59 添加自行指定分隔符功能，在array后添加=, 或者=|这些，指定分隔符为,或者|

2014年1月7日 15:10:51 修改digit array/string array中当value为空时，生成失败的问题

2014年1月8日 20:41:59 修改digit类型的值，使不会生成浮点数值。我们不会有浮点数。全部转换为整数
修改对于array类型，生成时不使用默认值，不然会有{0}这种非预期的输出

2014年1月21日 16:47:41 这里将配置生成的空项去除， 使用defaultValue = "nil" 作为是否置空的标志。

2014年1月23日 00:05:17 修复上面修改造成的bug.有些空的struct地方需要跳过生成

2014年6月19日 13:15:47 将客户端生成的配置，去掉date那一行。否则说会影响生成配置的差异包。。

2014年12月20日 14:03:01  简单重构此模块，将代码写得易懂些

# TODO :
1.  将表格的定义添加到生成的lua文件中，以注释形式,这样好像更方便观看lua配置
2.  格式对得不太整齐。并且有些不必要的分号和空格考虑去掉
3.  将struct结构内当没有填东西时，忽略掉不生成为空的

# LastChange: 2015-1-10
#========================================================================
'''
import os
import sys
from datetime import *
import time
import types
import commands
import re
import logging

reload(sys)
sys.setdefaultencoding("utf-8")

'''
有以下几种日志级别供选择，如果需要修改请设置LOG_LEVEL
logging.debug("debug")
logging.info("info")
logging.warning("warning")
logging.error("error")
logging.critical("critical")
'''
try:
    import xlrd
except:
    # print "not find xlrd in python sit-packages, import xlrd from local directory"
    libPath = os.path.join(os.path.dirname(os.path.realpath(__file__)), 'xlrd')
    sys.path.append(libPath)
    import xlrd

# LOG_LEVEL = logging.DEBUG
LOG_LEVEL = logging.WARNING

# 2013年8月16日 11:40:34 之前去掉了日志功能，现在重新添加上。虽然反复了，但目标是不一样的。
# 现在是为了控制在标准输出中的内容和兼顾调试
logging.basicConfig(level=LOG_LEVEL, format="[%(levelname)s]%(message)s")


class HeaderItem:
    pass


# 一个通用的表格输出函数，根据表格类型进行判断
class CardInterpreter:
    """通过excel配置生成配置的lua定义文件"""

    def __init__(self, xls_file_path, sheet_name, have_repeated_key, sub_key_col):
        self._xls_file_path = xls_file_path
        self._sheet_name = sheet_name
        self._sheet = None;

        logging.debug('xls_file_path:%s, sheet_name:%s' % (self._xls_file_path, self._sheet_name))

        # 是否有重复的key
        self._have_repeated_key = have_repeated_key
        # 当有重复key时，将以表格中第几列_sub_key_col列为sub key。如果为0则从0开始
        self._sub_key_col = sub_key_col

        # 初始化
        # 定义了表格的头部；那些结构。。用于后续数据的读取和解析
        self._header = []

        # 产生的lua结果集
        self._context = ""

        # 打开表格读取基本信息
        try:
            self._workbook = xlrd.open_workbook(self._xls_file_path)
        except BaseException, e:
            logging.error("open xls file(%s) failed!" % (self._xls_file_path))
            raise
        try:
            self._sheet = self._workbook.sheet_by_name(self._sheet_name)
        except Exception, e:
            logging.error("open sheet(%s) failed!" % (self._sheet_name))
            raise
        # 行数和列数
        self._row_count = self._sheet.nrows
        self._col_count = self._sheet.ncols
        logging.debug("sheet info|name:%s, row:%d col:%d "
                      % (self._sheet_name, self._row_count, self._col_count))

    def __del__(self):
        '''
        在析构时关闭日志文件

        '''

    def Interpreter(self, svr_or_client):
        """对外的接口, 解析xlsx中并生成配置文件"""
        logging.debug("==begin CardInterpreter[%s] in %s|row:%d,col:%d==",
                      self._sheet_name, self._xls_file_path, self._row_count, self._col_count)

        # 清空旧东西以便重入, 因为要生成客户端和服务器的配置
        self._header = []
        self._context = ""
        self._duplication_name_index = 1
        self._duplication_name_dic = {}
        self._default_name_dic = {}
        self._key_dic = {}

        # 解析配置
        self._ParseSheet(svr_or_client);

    def _Conv2IntStr(self, s):
        """将输入转为int的字符串,输入可能是str也可能是float"""
        logging.debug("_Conv2IntStr:%s,%s", s, str(type(s)))
        if str(type(s)) == "<type 'str'>":
            if s != "":
                if s == "nil":
                    return ""
                if -1 == s.find('.'):
                    return str(int(str(s)))
                else:
                    # 2014年1月8日 20:38:33 咱不生成float的东东
                    return str(int(float(str(s))))
            else:
                return '0'
        elif str(type(s)) == "<type 'unicode'>":
            # TODO 对数字进行一个校验()
            return s
        elif str(type(s)) == "<type 'int'>":
            return str(s)
        elif str(type(s)) == "<type 'float'>":
            return str(int(float(str(s))))
        else:
            logging.warning("type(s) = %s", str(type(s)))
            raise "conv2intstr err", s

    def _Conv2FloatStr(self, s):
        """将输入转为float的字符串,输入可能是str也可能是float"""
        if str(type(s)) == "<type 'str'>":
            if s != "":
                if s == "nil":
                    return ""
                if -1 == s.find('.'):
                    return str(float(str(s)))
                # else:
                #     #2014年1月8日 20:38:33 咱不生成float的东东
                #     return str(float(str(s))))
            else:
                return '0'
        elif str(type(s)) == "<type 'unicode'>":
            # TODO 对数字进行一个校验()
            return s
        elif str(type(s)) == "<type 'int'>":
            return str(s)
        elif str(type(s)) == "<type 'float'>":
            return str(float(s))
        else:
            logging.warning("type(s) = %s", str(type(s)))
            raise "conv2intstr err", s

    def _GetColHeader(self, col):
        """获得某一列的头部信息,一般为前面的3~4行
            返回：bool, (col, colName, outType, valueType, colDesc)
            如果是合法的头部，返回bool为true,后面跟的元组有意义。 否则为false,其后元组为空

            col 为列号
            colName为这一列的命名，英文，用于定义某项。如GoodsID
            outType为这一列用于输出的定义，指明是用于客户端，还是服务器或者都有用,取值B|S|C
            valueType为这一列的值的类型。指明如何解析这一列。[digit|string] [ |array] 或者是 struct
            colDesc是一个备注的描述
        """
        logging.debug("_GetColHeader:col=%d,total_col=%d" % (col, self._col_count))
        # 读取colName,这个可以默认在第一行,读不到或者为空认为此列无效
        if col >= self._col_count:
            return False, ()

        # NOTICE: 这里有个强约定，要求xls的前三行是按要求写的。
        # 第一行为name,第二行为out type, 第三行为value type

        # 2013年8月15日 22:06:10 让表格支持默认值  例:Name="kevin",当Name列为空时，使用默认值kevin
        name = str(self._sheet.cell(0, col).value).strip().split('=')

        colName = name[0].strip()
        if colName == "":
            return False, ()

        logging.debug("name: %s len:%d" % (name, len(name)))

        # 读取outType,这个可以默认在第二行;要根据valueType判断是否这个合法
        outType = str(self._sheet.cell(1, col).value).strip()

        # 读取valueType, 第三行,只能是定义的一些字符串
        valueType = str(self._sheet.cell(2, col).value).strip()
        logging.debug("valueType:%s" % (valueType))

        valueTypeLen = len(valueType.split('='))
        valueTypeExt = ""
        if valueTypeLen > 1:
            valueTypeExt = valueType.split('=')[1]
            valueType = valueType.split('=')[0]

        # 默认值
        defaultValue = 0

        if len(name) > 1:
            if valueType.split()[0] == "digit":
                defaultValue = self._Conv2IntStr(name[1].strip())
            elif valueType.split()[0] == "float":
                defaultValue = self._Conv2FloatStr(name[1].strip())
            else:
                defaultValue = name[1].strip()
        else:
            # 对于array形式，不设置其默认值了，不然会产生{0}这样的非预期的table
            if len(valueType) > 0 and valueType.split()[0] == "digit" and len(valueType.split()) <= 1:
                defaultValue = 0
                # defaultValue = "nil" #取消当没有定义默认值时，就不使用默认值，直接不生成此字段，以降低lua table的内存消耗
            else:
                # defaultValue = "nil"
                defaultValue = ""

        # 读取描述==没啥用。读进来方便看看而已
        colDesc = str(self._sheet.cell(3, col).value).strip()

        # 校验一下合法性
        vaild_types = [u"string", u"digit", u"float", u"time", u"string array", u"digit array", u"float array",
                       u"struct"]
        if colName == "":
            logging.debug("colName is empty??")
            return False, ()

        if not valueType in vaild_types:
            logging.debug("valueType(%s) is invaild??", valueType)
            return False, ()

        logging.debug("Succ: col:%d\tcolName:%s\toutType:%s\tvalueType:%s\tcolDesc:%s\tdefaultValue:%s"
                      % (col, colName, outType, valueType, colDesc, defaultValue))

        return True, (col, colName, outType, valueType, colDesc, defaultValue, valueTypeExt)

    def _ParseHeaderInfo(self, svr_or_client):
        """
            从表格中读取描述信息。以此定义出这个表格结构。其中使用了python的一些动态特性
        """
        col = 0
        while col < self._col_count:
            ret, header = self._GetColHeader(col)
            if not ret:
                logging.debug('jump..unkown col')
                col += 1
                continue

            # 创建对象。方便后面访问
            header_item = HeaderItem()
            setattr(header_item, "col", header[0])
            setattr(header_item, "colName", header[1])
            setattr(header_item, "outType", header[2])
            setattr(header_item, "valueType", header[3])
            setattr(header_item, "colDesc", header[4])
            setattr(header_item, "defaultValue", header[5])
            setattr(header_item, "valueTypeExt", header[6])

            # 跳过svr/client不匹配的列
            logging.debug("svr_or_client:%s outType:%s" % (svr_or_client, header_item.outType))
            # if svr_or_client == "server" and header_item.outType == "C":
            #     logging.debug("this is client config, server no need")
            #     col += 1
            #     continue
            # elif svr_or_client == "client" and header_item.outType == "S":
            if svr_or_client == "client" and header_item.outType == "S":
                logging.debug("this is server config, client no need")
                col += 1
                continue

            self._header.append(header_item)

            if header_item.valueType == "struct":
                struct_items = []
                struct_item_idx = 0
                struct_idx = 0

                # 如果是struct类型，才创建这个属性subNode
                setattr(header_item, "subNode", [])

                idx_max = int(float(header[2]))
                logging.debug('idx_max:%d' % (idx_max))
                key_name_list = []
                while struct_idx < idx_max:
                    col += 1
                    ret, header = self._GetColHeader(col)
                    if not ret:
                        logging.debug('jump..unkown col')
                        col += 1
                        continue
                    item = HeaderItem()
                    setattr(item, "col", header[0])
                    setattr(item, "colName", header[1])
                    setattr(item, "outType", header[2])
                    setattr(item, "valueType", header[3])
                    setattr(item, "colDesc", header[4])
                    setattr(item, "defaultValue", header[5])
                    setattr(item, "valueTypeExt", header[6])

                    # 添加到key_name_list中作为辨别这个struct的标识
                    key_name_list.append(item.colName)

                    logging.debug("struct_idx:%d col:%d" % (struct_idx, col))
                    # print "item-->", item.__dict__

                    if svr_or_client == "server" and item.outType == "C":
                        logging.debug("this is client config, server no need")
                        pass
                    elif svr_or_client == "client" and item.outType == "S":
                        logging.debug("this is server config, client no need")
                        pass
                    else:
                        struct_items.append(item);

                    # logging.debug('\t\tcol:',header[0], '\tcolName:', header[1], "\toutType:", header[2], "\tvalueType:", header[3],"\tcolDesc",header[4])

                    # 添加到上面的subNode列表中去
                    # struct_item.subNode.append(item)
                    struct_idx += 1

                    # 如果后面还有属于这个结构的，则重置一些参数
                    if struct_idx >= idx_max:  # 当已经到最后一个时，检查后续是否符合这个结构定义。如果符合就继续读
                        struct_item = HeaderItem()
                        setattr(struct_item, "subNode", struct_items)
                        setattr(struct_item, "col", header[0])
                        setattr(struct_item, "colName", "[%d]" % (struct_item_idx))
                        setattr(struct_item, "outType", header[2])
                        setattr(struct_item, "valueType", header[3])
                        setattr(struct_item, "colDesc", header[4])

                        # logging.debug('struct_item',struct_item)
                        header_item.subNode.append(struct_item)

                        # FIXME 偷懒只检查后面的一项是否和key_name_list[0]吻合就好了,暂时无大碍
                        ret, header = self._GetColHeader(col + 1)
                        # logging.debug('chech next struct:',header[1],key_name_list)
                        if ret and header[1] == key_name_list[0]:
                            struct_idx = 0  # 强制将循环的参数设置为0.很暴力吧
                            key_name_list = []
                            struct_items = []
                            struct_item_idx += 1
            col += 1

        # print "self._header", self._header

    def _ParseSheet(self, svr_or_client):
        # 解析表的头部定义, 这个是作为配置生成的重要根据
        self._ParseHeaderInfo(svr_or_client)

        """
        这个给我写代码时作参考
        [101011]={
            [1]={Index=1, IsTeamate=1, Content="洒家大好的心情吃酒，你们这群泼皮好生吵闹", },
            [2]={Index=2, IsTeamate=0, Content="擦，大爷我爱怎样就怎样，哪里由得你这个酒肉花和尚管得", },
            [3]={Index=3, IsTeamate=1, Content="废话…… 放马过来，洒家今天就让你知道花儿为何这样红", },
        },
       """

        # 从表格中读取内容到一个字符串中暂存。。话说字符串最多能存多少东西呢
        self._context = self._ReadContextFromSheet(svr_or_client)

    def GetResult(self):
        """
        在_ParseSheet之后，会将产生的lua配置的string放在成员_context中。通过这个来读取
        """
        return self._context

    def _ReadContextFromSheet(self, svr_or_client):
        """
            读取表格中的内容。要基于前面已经解析的HeaderInfo()来
        """
        logging.debug("ReadContextFromSheet|%s", svr_or_client)
        context_result = self._GetDefaultItem(svr_or_client)
        context_result += self._GetDuplicationItem(svr_or_client)
        context_result += "\nlocal " + self._sheet_name + " = {\n"

        last_key_id = 0
        repeated_key_index = 0

        for row in range(4, self._row_count):  # 从第4行开始是因为前三行被用来定义表结构了
            # 把头部定义的第一个合法项认为是key.当这个不存在或者不合法时，此行作废
            header_item = self._header[0]
            col = header_item.col  # col是在header_item对象动态添加的属性
            value = ""
            if self._sheet.cell(row, col).ctype == xlrd.XL_CELL_TEXT:
                value = self._sheet.cell(row, col).value.encode("utf-8")
            else:  # 其它类型都当作数字来处理
                value = self._sheet.cell(row, col).value

            if value == "":
                logging.debug("this key is null...continue")
                continue

            # 识别key的类型，是integer还是string
            key_is_integer = True
            try:
                this_key = self._Conv2IntStr(value)
                key_is_integer = True
            except:
                this_key = "\"%s\"" % (value)
                key_is_integer = False

            # 将key对应的头部输出来
            row_str_for_output = ""

            if this_key in self._key_dic:
                logging.error ("\nDuplicate key exists. Please check Excel!!!\nDuplicate key exists. Please check Excel!!!\nDuplicate key exists. Please check Excel!!!\n"+this_key)
                return

            if len(self._header) == 1:
                row_str_for_output += '\t[' + this_key + '] = gConstEmptyTable,'

            else:
                if self._have_repeated_key == 0:
                    row_str_for_output += '\t[' + this_key + '] = { '
                else:
                    if this_key != last_key_id:  # 有key的切换，需要把key输出来
                        if last_key_id != 0:
                            row_str_for_output += '\n\t\t},\n'
                        repeated_key_index = 0
                        row_str_for_output += '\t[' + this_key + '] = { '

                last_key_id = this_key
                self._key_dic[this_key] = True
                # 如果定义这个表有重复的key,则产生的表多一层。加个[0]={}   [1]={} 等
                if self._have_repeated_key == 1:
                    repeated_key_inside = 0
                    if self._sub_key_col == 0:
                        repeated_key_inside = repeated_key_index + 1
                    else:
                        sub_key_value = ""
                        if self._sheet.cell(row, self._sub_key_col).ctype == xlrd.XL_CELL_TEXT:
                            sub_key_value = self._sheet.cell(row, self._sub_key_col).value.encode("utf-8")
                        else:  # 其它类型都当作数字来处理
                            sub_key_value = self._sheet.cell(row, self._sub_key_col).value
                        repeated_key_inside = int(sub_key_value)

                    logging.debug(
                        "self._sub_key_col:%d|repeated_key_inside:%d" % (self._sub_key_col, repeated_key_inside))
                    # 2013年11月8日 13:33:15 使重复key时index从1开始，之前是从0
                    row_str_for_output += '\n\t\t[%d]={ ' % (repeated_key_inside)
                    repeated_key_index += 1

                # 如果key合法。尝试来解析这一行
                for header_item in self._header[1:]:
                    col = header_item.col
                    value = self._sheet.cell(row, col).value

                    if header_item.valueType == "struct":  # 解析结构体,使用递归吗？
                        row_str_for_output += "\n\t\t" + header_item.colName + ' = { \n'
                        struct_idx = 1
                        # logging.debug('in parse-->header_item',header_item.__dict__)
                        for item in header_item.subNode:
                            # row_str_for_output += "\t\t\t[%d] = {"%(struct_idx)
                            row_str_in_side = ""
                            row_str_in_side += "\t\t\t[%d] = { " % (struct_idx)
                            # logging.debug("\nitem",item.__dict__)
                            all_sub_item = ""
                            for in_item in item.subNode:
                                # logging.debug("\n\nin_item",in_item.__dict__)
                                # row_str_for_output += self._GetItemString(in_item, row)
                                all_sub_item += self._GetItemString(in_item, row, svr_or_client)
                            struct_idx += 1
                            # row_str_for_output += "},\n"
                            row_str_in_side += all_sub_item
                            row_str_in_side += " },\n"

                            if all_sub_item == "":
                                row_str_in_side = ""

                            row_str_for_output += row_str_in_side

                        row_str_for_output += '\t\t},\n\t\t'
                    else:
                        row_str_for_output += self._GetItemString(header_item, row, svr_or_client)

                # row_str_for_output += '\n\t},'
                row_str_for_output += '},'

                # 对于长度比较短的行，去除一些多余的符号,使生成格式更美观
                # if len(row_str_for_output) < 120:
                # row_str_for_output = row_str_for_output.replace('\n','')
                # row_str_for_output = row_str_for_output.replace('\t','')
                # row_str_for_output = '\t' + row_str_for_output

                # if self._have_repeated_key == 0:
                # row_str_for_output += '\n'

            row_str_for_output += '\n'

            context_result += row_str_for_output
            logging.info(row_str_for_output)

        # 在最后的最后， 需要多加一个},
        if self._have_repeated_key == 1:
            context_result += '\n\t\t},\n'

        context_result += "}"
        context_result += self._GetMetatable(svr_or_client)
        context_result += "\nreturn " + self._sheet_name

        logging.info(context_result)
        return context_result

    # 遍历列表重复项，声明为局部变量
    def _GetDuplicationItem(self, svr_or_client):

        if svr_or_client == "designer":
            return ""

        duplication = {}
        for row in range(4, self._row_count):  # 从第4行开始是因为前三行被用来定义表结构了
            for header_item in self._header[1:]:  # 从第二列开始查找
                value = self._sheet.cell(row, header_item.col).value
                if header_item.valueType == "digit array" or header_item.valueType == "string array" or header_item.valueType == "float array":  # 数字数组
                    strip_new_line = self._GetTableStr(header_item, value)

                    if header_item.colName not in self._default_name_dic or (
                            header_item.colName in self._default_name_dic and self._default_name_dic[
                        header_item.colName] != strip_new_line):
                        if strip_new_line != "gConstEmptyTable":
                            if strip_new_line in duplication:
                                duplication[strip_new_line][0] = duplication[strip_new_line][0] + 1
                            else:
                                lua_local_name = header_item.colName + '_' + str(self._duplication_name_index)
                                duplication[strip_new_line] = [1, lua_local_name]
                                self._duplication_name_index += 1

        has_local_result = False
        for item in duplication.keys():
            _value = duplication[item]
            if _value[0] > 1:
                has_local_result = True
                break

        if has_local_result:
            local_context_result = "\nlocal duplication = {\n"
            for item in duplication.keys():
                _value = duplication[item]
                if _value[0] > 1:
                    self._duplication_name_dic[item] = _value[1]
                    row_str_for_output = "\t" + _value[1] + ' = ' + item + ',\n'
                    local_context_result += row_str_for_output

            local_context_result += "}\n"
            return local_context_result
        else:
            return ""

    # 遍历列表每列重复项，声明为默认值
    def _GetDefaultItem(self, svr_or_client):
        local_context_result = ""
        if svr_or_client == "client":
            local_context_result = "\nlocal defaultValues = {\n"

            for header_item in self._header[1:]:  # 从第二列开始查找
                itemDefalutValues = {}
                for row in range(4, self._row_count):  # 从第4行开始是因为前三行被用来定义表结构了
                    value = self._sheet.cell(row, header_item.col).value
                    if value == "":  # 当为空时，使用默认值
                        if header_item.valueType == "digit array" or header_item.valueType == "string array" or header_item.valueType == "float array":  # 数组
                            value = "gConstEmptyTable"
                        else:
                            value = header_item.defaultValue

                    if header_item.valueType == "digit":  # 解析数字
                        value = self._Conv2IntStr(value)
                    elif header_item.valueType == "float":
                        value = self._Conv2FloatStr(value)
                    elif header_item.valueType == "string":  # 解析字符串
                        value = self._GetStringStr(value)
                    elif header_item.valueType == "time":
                        if str(value) == "":
                            value = 0
                        else:
                            value = str(int(time.mktime(time.strptime(str(value), "%Y-%m-%d %X"))))
                    elif header_item.valueType == "digit array" or header_item.valueType == "string array" or header_item.valueType == "float array":  # 数字数组
                        value = self._GetTableStr(header_item, value)

                    if value in itemDefalutValues:
                        itemDefalutValues[value] += 1
                    else:
                        itemDefalutValues[value] = 1
                    pass

                max = header_item.defaultValue
                maxCount = 0
                needDefalut = False
                for key in itemDefalutValues.keys():
                    if itemDefalutValues[key] > 1 and itemDefalutValues[key] > maxCount:
                        needDefalut = True
                        max = key
                        maxCount = itemDefalutValues[key]
                if needDefalut:
                    self._default_name_dic[header_item.colName] = max
                pass

            for key in self._default_name_dic.keys():
                local_context_result += "\t" + str(key) + " = " + str(self._default_name_dic[key]) + ",\n"

            local_context_result += "}\n"
        return local_context_result

    # lua原表设置代码
    def _GetMetatable(self, svr_or_client):
        if svr_or_client != "client":
            return ""

        return '''

do
    local base = {
        __index = defaultValues, --基类，默认值存取
        __newindex = function()
            --禁止写入新的键值
            error("Attempt to modify read-only table")
        end
    }
    for k, v in pairs(%s) do
        setmetatable(v, base)
    end
    base.__metatable = false --不让外面获取到元表，防止被无意修改
end
''' % (self._sheet_name)

    def _GetItemString(self, header_item, row, svr_or_client):

        logging.debug('%s :_GetItemString <--%d %d' % (svr_or_client, row, header_item.col))
        return_str = ""
        value = self._sheet.cell(row, header_item.col).value
        logging.debug('_GetItemString value is %s', value)
        if value == "":  # 当为空时，使用默认值
            # if header_item.defaultValue == "nil":
            # return ""
            # if svr_or_client == "client" :
            #     return ""
            # value = header_item.defaultValue
            # logging.debug('value is "" use default value:%s'%(str(value)))

            if header_item.valueType == "digit array" or header_item.valueType == "string array" or header_item.valueType == "float array":  # 数组
                value = "gConstEmptyTable"
            else:
                value = header_item.defaultValue

        if header_item.valueType == "digit":  # 解析数字
            value = self._Conv2IntStr(value)
        elif header_item.valueType == "float":
            value = self._Conv2FloatStr(value)
        elif header_item.valueType == "string":  # 解析字符串
            value = self._GetStringStr(value)
        elif header_item.valueType == "time":
            if str(value) == "":
                value = 0
            else:
                value = str(int(time.mktime(time.strptime(str(value), "%Y-%m-%d %X"))))
        elif header_item.valueType == "digit array" or header_item.valueType == "string array" or header_item.valueType == "float array":  # 数字数组
            value = self._GetTableStr(header_item, value)

        logging.debug('_GetItemString-->%s' % (return_str))

        if header_item.colName in self._default_name_dic and value == self._default_name_dic[header_item.colName]:
            return ""
        else:
            if value in self._duplication_name_dic:
                return_str += header_item.colName + ' = duplication.' + self._duplication_name_dic[
                    value] + ', '
            else:
                return_str += header_item.colName + ' = ' + value + ', '
            return return_str

    def _GetTableStr(self, header_item, value):
        if value == "gConstEmptyTable":
            return "gConstEmptyTable"
        return_str = "{ "
        # 在数字数组中，一般内容是1;2;3 或者 1,2,3,4 或者  1|2|3|4 即有这三种分隔符
        # FIXME 本来想做智能感知，用于判断到底用哪个分隔符好。目前暂时认为这几种都是分隔符吧,没大碍
        logging.debug('array value: %s' % (str(value)))
        for_split_str = str(value)
        if len(for_split_str) < 1:
            return "gConstEmptyTable"
        split_char = ';,|:'
        # 当有定义分隔符时，以这里定义的为准
        if header_item.valueTypeExt != "":
            split_char = header_item.valueTypeExt

        if split_char.find(for_split_str[-1]) != -1:  # 最后一个字符是分隔符时，去掉它
            for_split_str = for_split_str[:-1]
            logging.debug('for_split_str: %s' % (for_split_str))

        split_value = re.split('[' + split_char + ']', str(for_split_str))
        idx = 0
        for item in split_value:
            if header_item.valueType == "digit array":
                logging.debug('digit array value item: %s' % (item))
                # return_str +=  '[%d] = %s, '%(idx, self._Conv2IntStr(item))
                return_str += '%s, ' % (self._Conv2IntStr(item))
            elif header_item.valueType == "string array":
                logging.debug('string array value item: %s' % (item))
                # return_str += '[%d] = "%s",'%(idx, unicode(item))
                return_str += '"%s",' % (unicode(item).replace(".0", ""))
            elif header_item.valueType == "float array":
                logging.debug('float array value item: %s' % (item))
                return_str += '%s, ' % (item)
            else:
                raise "header_item.valueType:%s err" % (header_item.valueType)
            idx += 1

        return_str += '}'
        return return_str

    def _GetStringStr(self, value):
        return '"' + unicode(value).replace("\n", "") + '"'


def OutputFileHeader(xls_file_name, output_file_name, sheet_name, is_client):
    """生成lua文件的描述信息"""

    file_name = output_file_name
    output = """--[[

 @file:       %s
 @source xls: %s
 @sheet name: %s
 @brief:      this file was create by tools, DO NOT modify it!
 @author:     kevin

]]--
""";

    output_str = output % (file_name, xls_file_name, sheet_name.upper())

    # output_str += "\nmodule(\"config\", package.seeall)\n\n\n"

    # return output%(file_name, xls_file_name, sheet_name.upper(), mt)
    return output_str


def Write2File(svr_or_client, xls_name, file_name, context):
    curAbsDir = os.path.dirname(os.path.abspath(os.path.curdir))
    fname, fename = os.path.splitext(xls_name)
    fbase = os.path.basename(xls_name)
    """输出到文件"""

    svr_or_client = svr_or_client.strip().lower()
    if svr_or_client == "server":
        path = curAbsDir + "/config_server/config/" + fbase.replace(fename, "") + "/"
    elif svr_or_client == "client":
        path = curAbsDir + "/config_client/config/" + fbase.replace(fename, "") + "/"
    else:
        path = curAbsDir + "/config_designer/config/" + fbase.replace(fename, "") + "/"

    if os.path.exists(path) == False:
        os.makedirs(path)

    open_lua_file = path + file_name
    lua_file = open(open_lua_file, "w+")
    lua_file.write(context)
    lua_file.close()


def GetFileName(svr_or_client, sheet_name):
    # if svr_or_client.strip().lower() == "server":
    #     file_name = "config_svr_" + sheet_name.strip().lower() + ".lua"
    # else:
    #     file_name = "config_client_" + sheet_name.strip().lower() + ".lua"

    if svr_or_client.strip().lower() == "server":
        file_name = sheet_name.strip().lower() + ".lua"
    else:
        file_name = sheet_name.strip().lower() + ".lua"
    return file_name


# def verify(num):
#    return ('float', 'int')[round(float(num)) == float(num)]

def TestLuaFile(filename):
    command = "lua output/" + filename
    status, output = commands.getstatusoutput(command)
    if (status != 0):
        logging.debug(
            "\x1B[0;31;40m WARNING: \e[1;35m Test " + filename + "\t--> FAILED!" + str(status) + output + "\x1B[0m")
        print "\x1B[0;31;40m[ERROR]: Test " + filename + " --> FAILED!\x1B[0m"
        print  "\x1B[0;35;40m\t" + str(status) + output + "\x1B[0m"
        # sys.exit(-1)
    else:
        print "\x1B[0;32;40m[SUCCESS]: Test " + filename + " --> OK\x1B[0m"
