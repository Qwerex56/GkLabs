using System.Drawing;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Window = Silk.NET.Windowing.Window;

namespace SilkNg;

public class Game {
    private static IWindow _window = null!;
    private static GL _gl = null!;

    public Game() {
        _window = Window.Create(WindowOptions.Default with { Size = new(800, 600) });

        _window.Load += OnLoad;
        _window.Render += OnRender;

        _window.Run();
    }

    private static void OnLoad() {
        _gl = _window.CreateOpenGL();

        // Background color
        _gl.ClearColor(Color.CornflowerBlue);
    }

    private static void OnRender(double dt) {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        // DrawRectangle(new(_window.Size.X / 3f, -_window.Size.Y / 3f),
        //     new(_window.Size.X / 3f, _window.Size.Y / 3f),
        //     Color.Coral);
        //
        // #region second fractal iteration
        //
        // DrawRectangle(new(_window.Size.X / 9f, -_window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(4f * _window.Size.X / 9f, -_window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(7f * _window.Size.X / 9f, -_window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(_window.Size.X / 9f, -4f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(4f * _window.Size.X / 9f, -4f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(7f * _window.Size.X / 9f, -4f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(_window.Size.X / 9f, -7f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(4f * _window.Size.X / 9f, -7f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        // DrawRectangle(new(7f * _window.Size.X / 9f, -7f * _window.Size.Y / 9f),
        //     new(_window.Size.X / 9f, _window.Size.Y / 9f),
        //     Color.Coral);
        //
        //
        // DrawRectangle(new(_window.Size.X / 27f, -_window.Size.Y / 27f),
        //     new(_window.Size.X / 27f, _window.Size.Y / 27f),
        //     Color.Coral);
        // #endregion

        DrawFractal();
        
        // DrawTriangle();
    }


    private static unsafe void DrawTriangle() {
        var vao = _gl.GenVertexArray();
        _gl.BindVertexArray(vao);

        var vertices = new[] {
            0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f
        };

        var vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        fixed (float* buf = vertices) {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf,
                BufferUsageARB.StaticDraw);
        }

        var indices = new uint[] {
            0, 1, 2
        };

        var ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        fixed (uint* buf = indices) {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf,
                BufferUsageARB.StaticDraw);
        }

        const string vertexCode = """
                                  #version 330 core
                                  layout(location = 0) in vec3 aPosition;
                                  layout(location = 1) in vec3 aColorCoord;

                                  out vec3 frag_position;

                                  void main() {
                                      gl_Position = vec4(aPosition, 1.0f);
                                      
                                      frag_position = aColorCoord;
                                  }
                                  """;

        const string fragmentCode = """
                                    #version 330 core

                                    in vec3 frag_position;

                                    out vec4 out_color;

                                    void main() {
                                      out_color = vec4(frag_position.x, frag_position.y, frag_position.z, 1.0);
                                    }
                                    """;

        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var vStatus);

        if (vStatus != (int)GLEnum.True) {
            throw new("Error compiling vertex shader");
        }

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out var fStatus);

        if (fStatus != (int)GLEnum.True) {
            throw new("Error compiling fragment shader");
        }

        var program = _gl.CreateProgram();

        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);

        _gl.LinkProgram(program);
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var lStatus);

        if (lStatus != (int)GLEnum.True) {
            throw new("Error linking program");
        }

        _gl.DetachShader(program, vertexShader);
        _gl.DetachShader(program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float),
            (void*)0);

        const uint colorLoc = 1;
        _gl.EnableVertexAttribArray(colorLoc);
        _gl.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float),
            (void*)(3 * sizeof(float)));

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        _gl.BindVertexArray(vao);
        _gl.UseProgram(program);
        _gl.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static unsafe void DrawRectangle(Vector2 position, Vector2 size, Color color, float deform = 0.0f) {
        deform = Math.Clamp(deform, 0.0f, 1.0f);

        var vao = _gl.GenVertexArray();
        _gl.BindVertexArray(vao);

        // Create a rectangle vertex array
        // var vertices = new[] {
        //     position.X / _window.Size.X * (2.0f - deform), position.Y / _window.Size.Y * (2.0f), 0.0f, // p1p
        //     color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c
        //     (position.X + size.X) / _window.Size.X * (2.0f - deform), position.Y / _window.Size.Y * (2.0f - deform),
        //     0.0f, // p1p
        //     color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c
        //     (position.X + size.X) / _window.Size.X * (2.0f), (position.Y + size.Y) / _window.Size.Y * (2.0f - deform),
        //     0.0f, // p1p
        //     color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c
        //     position.X / _window.Size.X * (2.0f), (position.Y + size.Y) / _window.Size.Y * (2.0f - deform), 0.0f, // p1p
        //     color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c
        // };

        var vertices = new[] {
            2f * position.X / _window.Size.X /*- 2f * size.X / _window.Size.X*/ - 1f,
            2f * position.Y / _window.Size.Y /*+ 2f * size.Y / _window.Size.Y*/ + 1f,
            0f, // p1p
            color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c

            2f * position.X / _window.Size.X + 2f * size.X / _window.Size.X - 1f,
            2f * position.Y / _window.Size.Y /*+ 2f * size.Y / _window.Size.Y*/ + 1f,
            0f, // p1p
            color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c

            2f * position.X / _window.Size.X + 2f * size.X / _window.Size.X - 1f,
            2f * position.Y / _window.Size.Y - 2f * size.Y / _window.Size.Y + 1f,
            0f, // p1p
            color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c

            2f * position.X / _window.Size.X /*- 2f * size.X / _window.Size.X*/ - 1f,
            2f * position.Y / _window.Size.Y - 2f * size.Y / _window.Size.Y + 1f,
            0f, // p1p
            color.R / 256.0f, color.G / 256.0f, color.B / 256.0f, // p1c
        };

        var vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        fixed (float* buf = vertices) {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buf,
                BufferUsageARB.StaticDraw);
        }

        var indices = new uint[] {
            0, 1, 3,
            1, 2, 3
        };

        var ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        fixed (uint* buf = indices) {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf,
                BufferUsageARB.StaticDraw);
        }

        const string vertexCode = """
                                  #version 330 core
                                  layout(location = 0) in vec3 aPosition;
                                  layout(location = 1) in vec3 aColorCoord;

                                  out vec3 frag_position;

                                  void main() {
                                      gl_Position = vec4(aPosition, 1.0f);
                                      
                                      frag_position = aColorCoord;
                                  }
                                  """;

        const string fragmentCode = """
                                    #version 330 core

                                    in vec3 frag_position;

                                    out vec4 out_color;

                                    void main() {
                                      out_color = vec4(frag_position.x, frag_position.y, frag_position.z, 1.0);
                                    }
                                    """;

        var vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out var vStatus);

        if (vStatus != (int)GLEnum.True) {
            throw new("Error compiling vertex shader");
        }

        var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out var fStatus);

        if (fStatus != (int)GLEnum.True) {
            throw new("Error compiling fragment shader");
        }

        var program = _gl.CreateProgram();

        _gl.AttachShader(program, vertexShader);
        _gl.AttachShader(program, fragmentShader);

        _gl.LinkProgram(program);
        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var lStatus);

        if (lStatus != (int)GLEnum.True) {
            throw new("Error linking program");
        }

        _gl.DetachShader(program, vertexShader);
        _gl.DetachShader(program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const uint positionLoc = 0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);

        const uint colorLoc = 1;
        _gl.EnableVertexAttribArray(colorLoc);
        _gl.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float),
            (void*)(3 * sizeof(float)));

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        _gl.BindVertexArray(vao);
        _gl.UseProgram(program);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    private static void DrawFractal(int n = 3) {
        for (var i = 1; i <= n; ++i) {
            for (var j = 1; j <= float.Pow(3, i) / 3; ++j) {
                for (var k = 1; k <= float.Pow(3, i) / 3; ++k) {
                    var posX = k * 3f - 2f;
                    var posY = j * 3f - 2f;

                    var scale = float.Pow(3f, i);
                    
                    var randColor = Color.FromArgb(Random.Shared.Next(0, 255), Random.Shared.Next(0, 255),
                        Random.Shared.Next(0, 255));
                    
                    DrawRectangle(new(posX * _window.Size.X / scale, -posY * _window.Size.Y / scale),
                        new(_window.Size.X / scale, _window.Size.Y / scale),
                        randColor);
                }
            }
        }
    }
}