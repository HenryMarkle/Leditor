#version 330

uniform sampler2D inputTexture;

in vec4 fragColor;
in vec2 fragTexCoord;

out vec4 FragColor;

void main()
{
    vec4 white = vec4(1.0, 1.0, 1.0, 1.0);

    vec4 c = texture(inputTexture, fragTexCoord);

    if (c == white) discard;

    FragColor = fragColor;
}
