#version 330

uniform sampler2D inputTexture;
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
        newColor = vec4(c.r + depthTint, c.g + depthTint, c.b + depthTint, c.a);
    }

if (newColor.a == 0.0) discard;

    FragColor = newColor;
}