#version 330

uniform sampler2D inputTexture;

uniform int inverted;

in vec4 fragColor;
in vec2 fragTexCoord;

out vec4 FragColor;

void main()
{
    vec4 white = vec4(1.0, 1.0, 1.0, 1.0);
    
    vec4 c = texture(inputTexture, fragTexCoord);

    if (c == white)
    {
        if (inverted != 0)
        {
            FragColor = vec4(0);
        }
        else
        {
            FragColor = white;
        }
    }
    else 
    {
        if (inverted != 0)
        {
            FragColor = white;
        }
        else
        {
            FragColor = vec4(0);
        }
    }
}
