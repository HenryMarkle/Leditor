#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

// 0 - 29
uniform int depth;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 c = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));

    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || c.a == 0.0 || fragColor.a == 0.0 || depth > 29) {
        discard;
    }

    vec4 paletteColor = vec4(0);

    if (c.b == 1.0) {
        paletteColor = texture(paletteTexture, vec2(depth/32.0, 2.0/16.0));
    } else if (c.g == 1.0) {
        paletteColor = texture(paletteTexture, vec2(depth/32.0, 3.0/16.0));
    } else if (c.r == 1.0) {
        paletteColor = texture(paletteTexture, vec2(depth/32.0, 4.0/16.0));
    }

    paletteColor.a = fragColor.a;

    FragColor = paletteColor;
}