#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

// 0 - 29
uniform int depth;

// 0 - highlight
// 1 - base
// 2 - shadow
uniform int shading;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    float depthTint = 0.03 * depth;
    vec4 c = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));

    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || c.a == 0.0 || fragColor.a == 0.0) {
        discard;
    }
    
    vec4 paletteColor = texture(paletteTexture, vec2(depth / 32.0, ((shading % 3) + 2) / 16.0));

    paletteColor.a = fragColor.a;

    FragColor = paletteColor;
}