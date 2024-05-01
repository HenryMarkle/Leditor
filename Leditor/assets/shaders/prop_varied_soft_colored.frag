#version 330

uniform sampler2D inputTexture;

uniform float varWidth;
uniform float height;
uniform int variation;
uniform int depth;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    float newXCoord = fragTexCoord.x * varWidth + (variation * varWidth);
    float newYCoord = fragTexCoord.y * height;
    
    vec4 c = texture(inputTexture, vec2(newXCoord, newYCoord));

    if ((c.b == 1.0 && c.g == 1.0 && c.r == 1.0) || c.g == 0.0) {
        discard;
    }

    FragColor = vec4(fragColor.r * c.g, fragColor.g * c.g, fragColor.b * c.g, fragColor.a);
}