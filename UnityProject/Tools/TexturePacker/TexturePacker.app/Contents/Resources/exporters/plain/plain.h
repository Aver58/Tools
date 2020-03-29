Simple plain text exporter for demo purposes

This file might contain additional information

SmartUpdateHash: {{smartUpdateKey}}

{% for result in allResults %}
RESULT = { {% for texture in result.textures %}
    TEXURE = {
        area             = {{texture.area}}
        size             = {{texture.size.width}} x {{texture.size.height}}
        absoluteFileName = "{{texture.absoluteFileName}}"
        trimmedName      = "{{texture.trimmedName}}"
        fullName         = "{{texture.fullName}}"
        ALL_SPRITES      = { {% for sprite in texture.allSprites %}
            "{{sprite.trimmedName}}" x={{sprite.frameRect.x}} y={{sprite.frameRect.y}} w={{sprite.frameRect.width}} h={{sprite.frameRect.height}} {% endfor %}
        }
{% endfor %}
    NON_FITTING_SPRITES  = { {% for sprite in result.notFittingSprites %}
        {{sprite.trimmedName}}{% endfor %}
    }
}
{% endfor %}
