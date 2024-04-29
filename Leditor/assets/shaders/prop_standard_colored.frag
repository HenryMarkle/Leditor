#version 330

uniform sampler2D inputTexture;

uniform vec4 tint;
uniform int layerNum;
uniform float layerHeight;
uniform float width;
uniform int depth;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 newColor = vec4(0);

    for (int l = layerNum - 1; l > -1; l--) {
        float depthTint = (depth + l) * 0.03;
        float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);

        vec2 newFragTexCoord = vec2(fragTexCoord.x * width, currentHeight);

        vec4 c = texture(inputTexture, newFragTexCoord);

        if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;
        
        if (c.b == 1.0) {
            newColor = vec4(tint.r - 0.1, tint.g - 0.1, tint.b - 0.1, tint.a);
        }
        else if (c.g == 1.0) {
            newColor = vec4(tint.r - 0.3, tint.g - 0.3, tint.b - 0.3, tint.a);
        }
        else if (c.r == 1.0) {
            newColor = vec4(tint.r - 0.6, tint.g - 0.6, tint.b - 0.6, tint.a);
        } else {
            newColor = c;
        }
        
        newColor = vec4(newColor.r + depthTint, newColor.g + depthTint, newColor.b + depthTint, newColor.a);
    }

    if (newColor.a == 0.0) discard;

    FragColor = newColor;
}