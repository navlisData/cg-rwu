#version 330 core

layout (location = 0) in vec2 aPosition;

layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 uMVP;

void main(void)
{
    texCoord = aTexCoord;

    gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
}