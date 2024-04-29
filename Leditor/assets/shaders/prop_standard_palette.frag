#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

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
        float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);

        vec2 newFragTexCoord = vec2(fragTexCoord.x * width, currentHeight);

        vec4 c = texture(inputTexture, newFragTexCoord);

        if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;
    
        float depthCoord = (depth + l);

        if (depthCoord > 29.0) continue;

        if (c.b == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 2.0/16.0));
        } 
        else if (c.g == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 3.0/16.0));

        } 
        else if (c.r == 1.0) {
            newColor = texture(paletteTexture, vec2(depthCoord/32.0, 4.0/16.0));

        } else {
            newColor = c;
        }
    }

if (newColor.a == 0.0) discard;

    FragColor = newColor;
}