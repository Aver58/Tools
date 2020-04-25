rem 设置BMFont软件安装路径
set BMFont_PATH=F:\AL\Assets\Plugins\Scripts\Editor\BMFont\BMFont

rem 设置文本、配置文件以及输出fnt文件的路径
set TEXT_PATH=F:\AL\Assets\Plugins\Scripts\Editor\BMFont\sourcePNG

rem targetChars.txt 文件是存放需要的文本{例如：123456789D}，
rem config.bmfc 文件存放图片文件的具体路径
rem font.fnt 输出文件的文件名
"%BMFont_PATH%\bmfont.exe" -t %TEXT_PATH%\targetChars.txt -c "%TEXT_PATH%\config.bmfc"  -o "%TEXT_PATH%\font.fnt"

