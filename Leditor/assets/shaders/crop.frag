#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

uniform sampler2D inputTexture;
uniform sampler2D maskTexture;

void main()
{
    vec2 vFlipped = vec2(fragTexCoord.x, 1.0 - fragTexCoord.y);
    vec4 maskColor = texture(maskTexture, fragTexCoord);

    if (maskColor.r == 1.0 && maskColor.g == 1.0 && maskColor.b == 1.0) {
        discard;
    }
    
    FragColor = texture(inputTexture, vFlipped);
}
