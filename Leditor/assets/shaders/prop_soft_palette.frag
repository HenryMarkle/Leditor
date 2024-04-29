#version 330

uniform sampler2D inputTexture;
uniform sampler2D paletteTexture;

uniform int depth;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    float depthTint = 0.03 * depth;
    vec4 c = texture(inputTexture, fragTexCoord);
    
    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || (c.r == 0.0 && c.g == 0.0 && c.b == 0.0)) {
        discard;
    }

    float depthCoord = (1.0 - c.g + depth/30.0);

    if (depthCoord > 0.9375) discard;

    FragColor = texture(paletteTexture, vec2(depthCoord / (30.0/32.0), 3.0/16.0));
}