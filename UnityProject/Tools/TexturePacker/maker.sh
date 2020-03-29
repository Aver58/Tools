#!/bin/bash

TexturePacker="/Applications/TexturePacker.app/Contents/MacOS/TexturePacker"

PATH_SRC=$1
PATH_DST=$2
TRIN_MODE=None
if [ $3 == 1 ]; then
	TRIN_MODE=Trim
fi

SHAPE_PADDING=$4

DATA_FILE=${PATH_DST}.txt
SHEET_FILE=${PATH_DST}.png

$TexturePacker --smart-update $PATH_SRC --data $DATA_FILE --format unity --sheet $SHEET_FILE --max-size 2048 --size-constraints POT --force-squared --disable-rotation --trim-mode $TRIN_MODE --trim-margin 0 --extrude 1  --border-padding 0 --shape-padding $SHAPE_PADDING
