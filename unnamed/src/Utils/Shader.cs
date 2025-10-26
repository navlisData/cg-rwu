using OpenTK.Graphics.OpenGL;

namespace unnamed.Utils;

public static class Shader
{
    public static int Setup(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexPath);
        string fragmentShaderSource = File.ReadAllText(fragmentPath);

        int vertex = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertex, vertexShaderSource);
        GL.CompileShader(vertex);
        CheckCompilation(vertex);

        int fragment = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragment, fragmentShaderSource);
        GL.CompileShader(fragment);
        CheckCompilation(fragment);

        int shader = GL.CreateProgram();
        GL.AttachShader(shader, vertex);
        GL.AttachShader(shader, fragment);
        GL.LinkProgram(shader);

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);

        return shader;
    }

    private static void CheckCompilation(int shader)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
        if (status != 0)
        {
            return;
        }

        Console.WriteLine(GL.GetShaderInfoLog(shader));
        throw new Exception("Shader compilation failed!");
    }
}