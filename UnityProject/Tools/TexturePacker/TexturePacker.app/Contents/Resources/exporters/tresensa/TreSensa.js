// Created with TexturePacker http://www.codeandweb.com/texturepacker
// {{smartUpdateKey}}
TGE.AssetManager.SpriteSheets["{{texture.trimmedName}}"] = {
{% for sprite in allSprites %}
	"{{sprite.trimmedName}}":[{{sprite.frameRect.x}}, {{sprite.frameRect.y}}, {{sprite.frameRect.width}}, {{sprite.frameRect.height}}{% if sprite.trimmed %}, {{sprite.sourceRect.x}}, {{sprite.sourceRect.y}}, {{sprite.untrimmedSize.width}}, {{sprite.untrimmedSize.height}}{% endif %}]{% if not forloop.last %}, {% endif %}{% endfor %}
};
