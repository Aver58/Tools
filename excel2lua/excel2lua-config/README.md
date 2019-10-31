##excel2lua说明

---

###使用依赖
* python2.7+
* python一个库xlrd, 已经作为git submodule引用了。

```
    cd xlrd
    git submodule init
    git submodule update
```

###作用
* 支持xls/xlsx的导出，视xlrd版本而定
* 对于excel的结构定义，见示例中的excel/Dungeon.xls

###思路

###核心部分：
* excel2lua_mod.py  
`用于将某个excel中某个sheet转换为lua的配置table`

* excel2lua_final.py  
`调用excel2lua_mod.py,传递一些参数。此模块可单独使用。`

###外围部分  
* get_xls_changelist.sh 通过svn的对比工具，查找出有变化的xls, 供增量生成配置 
* excel_cmd.sh 这个是用于查找某个xls文件它的生成配置的命令
* rapid_do.sh/do_all.sh 主(控)逻辑脚本


###后记
* 因为当时写此脚本为临时需求，没考虑太多的结构方面东西，代码quick&dirty
