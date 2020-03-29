@echo off

rem SET TexturePacker="C:/Program Files/TexturePacker/TexturePacker.exe"
SET TexturePacker="./Tools/TexturePacker/TexturePacker_Win32/bin/TexturePacker.exe"

SET PATH_SRC=%1%
SET PATH_DST=%2%
SET TRIN_MODE=None
SET SHAPE_PADDING=%4%

SET DATA_FILE=%PATH_DST%.txt
SET SHEET_FILE=%PATH_DST%.png

if %3% == 1 SET TRIN_MODE=Trim

./Tools/TexturePacker/TexturePacker_Win32/bin/TexturePacker.exe --smart-update %PATH_SRC% --data %DATA_FILE% --format unity --sheet %SHEET_FILE% --max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode %TRIN_MODE% --trim-margin 0 --extrude 1
