#!/bin/bash
#========================================================================
#   FileName: rapid_do.sh
#     Author: kevinlin
#       Desc: 快速生成最新配置, 依赖于其它脚本和文件协助 
#             和svn关联，计算差异。若非此情况不能必执行些脚本
#                @get_xls_changelist.sh -->取差异xlsx
#                @excel_cmd.txt  --> 各个配置生成规则
#     Create: 2014-12-15 12:04:37
#    History: 
#   FIXME:  在取svn commit信息时， 有个报错; svn: When specifying working copy paths, only one target may be given
# LastChange: 2014-12-15 12:04:37
#========================================================================

#获得有更新的文件列表
change_list=change_list.txt
all_xls_version=all_xls_version.txt

./get_xls_changelist.sh > ${change_list}

while read line 
do
    file_name=`echo $line | tr -d ' '`
    #显示本次更新修改的内容
    version=`grep $file_name ${all_xls_version}  | awk '{print $2}'`
    echo -e -n "\e[1;31m|---- $file_name --->"
    svn log  -r${version}  excel/${file_name}  | sed -n '4,4p'
    echo -e "\e[0m"

    #查找变化的xls对应的配置并执行成生命令
    grep ${file_name} excel_cmd.txt  | grep -v "^[ ]*#.*" | while read line; do echo $line; $line; done
    echo

done < ${change_list}




