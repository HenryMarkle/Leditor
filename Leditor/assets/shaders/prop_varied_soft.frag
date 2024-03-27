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
    
    float depthTint = 0.03 * depth;
    
    vec4 c = texture(inputTexture, vec2(newXCoord, newYCoord));

    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || (c.r == 0.0 && c.g == 0.0 && c.b == 0.0)) {
        discard;
    }

    FragColor = vec4(c.r + depthTint, c.g + depthTint, c.b + depthTint, c.a);
}