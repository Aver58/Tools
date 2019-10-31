#! /usr/bin/env python
# coding=utf-8
'''
#========================================================================
#   FileName: excel2lua_final.py
#     Author: kevinlin
#       Desc: 调用excel2lua_mod.py来生成配置
#      Email: linjiang1205 AT qq.com
# LastChange: 2014-12-20 16:55:32
#========================================================================
'''
import os
import sys
import commands
from excel2lua_mod import *
import getopt


def usage():
    print '''
Usage: %s excel_file sheet_name ["repeated=[0|1]", "subkey=NUM" , "output=[c|s|cs|sc]"]
   
   excel_file:   the source file of config
   sheet_name:   the sheet you want convent to lua
     repeated:   is key repeated??
                    0 means no repeated key inside
                    1 means the key maybe repeated, then parse them as one table
       subkey:   table nested. point out which column is the sub key
       output:   [c|s|cs|sc] client or svr or both?? 
                    c  means client, has some different with s
                    s  means server, has some different with c

    ''' % (sys.argv[0])


if __name__ == '__main__':
    if len(sys.argv) < 3:
        usage()
        sys.exit(-1)

    xls_file_name = sys.argv[1]
    # 读取sheet名，转为大写字母
    sheet_name = sys.argv[2].upper();
    # 是否有重复的key，为0时表示key不重复。为1时有重复key,将对某个key产生多一层的table
    have_repeated_key = 0
    # 当有重复的key时，这里指定index从0开始，还是以第二列的数值为index(默认从0开始。参考Dungen.xls)
    sub_key_col = 0
    # 输出目标、client/svr
    output = ""

    try:
        opt, args = getopt.getopt(sys.argv[3:], "hr:s:o:", ["help", "repeated=", "subkey=", "output="])
    except getopt.GetoptError, err:
        print "err:", (err)
        usage()
        sys.exit(-1)

    for op, value in opt:
        if op == "-h":
            usage()
        elif op == "-r" or op == "--repeated":
            have_repeated_key = int(value)
        elif op == "-s" or op == "--subkey":
            sub_key_col = int(value)
        elif op == "-o" or op == "--output":
            output = value

    logging.debug("have_repeated_key:%d|sub_key_col:%d|output:%s" % (have_repeated_key, sub_key_col, output))

    outputstr = {"c": "client", "s": "server", "d": "designer"}

    for i in output:
        if not outputstr.has_key(i):
            usage()
            sys.exit(-2)

        parser = CardInterpreter(xls_file_name, sheet_name, have_repeated_key, sub_key_col)
        parser.Interpreter(outputstr[i])
        sheet_name_lower = sheet_name.strip().lower();
        file_name = sheet_name_lower + ".lua"
        context = OutputFileHeader(xls_file_name, file_name, sheet_name_lower, False)
        context += parser.GetResult()
        Write2File(outputstr[i], xls_file_name.lower(), file_name, context)

        # 试运行产生的lua脚本看是否会报错
        # TestLuaFile(file_name)

    print "\nExport To Lua Succeed"
