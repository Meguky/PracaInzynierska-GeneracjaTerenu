﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaInzynierska
{
    class MainWindow : GameWindow
    {


        uint[] indices = {
            0, 1, 3,
            1, 2, 3
        };

        float[] vertices = {
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        float[] texCoords = {
            0.0f, 0.0f,
            1.0f, 0.0f,
            0.5f, 1.0f
        };

        int vertexBufferObject;
        int vertexArrayObject;
        int elementBufferObject;
        Shader shader;
        Texture texture;
        Camera camera;
        float time;
        bool firstMove = true;
        Vector2 lastPos;
        Vector4 color = new Vector4(1f,1f,1f,1f);
        public MainWindow(int width, int height, string title) : base(width,height,GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            shader = new Shader("../../Shaders/shader.vert", "../../Shaders/shader.frag");
            shader.Use();

            texture = new Texture("../../container.png");
            texture.Use();

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aPosition"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aTexCoord"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aTexCoord"), 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            camera = new Camera(Vector3.UnitZ * 3, Width / (float)Height);

            CursorVisible = false;
            GL.BindVertexArray(0);
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            time += 4.0f * (float)e.Time;
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            texture.Use();
            shader.Use();
            GL.BindVertexArray(vertexArrayObject);
            Matrix4 model;
            for (int i = 0; i < 50; i++)
            {
                model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(time * 50 + i*5));
                model *= Matrix4.CreateTranslation(new Vector3(i + 1, 0f, 0f));
                shader.SetMatrix4("model", model);
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            }


            Console.WriteLine(color);

            shader.SetVector4("lightColor", color);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            camera.AspectRatio = Width / (float)Height;

            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }

            var input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            if (input.IsKeyDown(Key.W))
                camera.Position += camera.front * camera.speed * (float)e.Time; 
            if (input.IsKeyDown(Key.S))
                camera.Position -= camera.front * camera.speed * (float)e.Time;
            if (input.IsKeyDown(Key.A))
                camera.Position -= camera.right * camera.speed * (float)e.Time;
            if (input.IsKeyDown(Key.D))
                camera.Position += camera.right * camera.speed * (float)e.Time;
            if (input.IsKeyDown(Key.Space))
                camera.Position += camera.up * camera.speed * (float)e.Time;
            if (input.IsKeyDown(Key.LShift))
                camera.Position -= camera.up * camera.speed * (float)e.Time;
            if (input.IsKeyDown(Key.E))
                camera.speed += 0.5f;
            if (input.IsKeyDown(Key.Q))
                camera.speed -= 0.5f;

            var mouse = Mouse.GetState();

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - lastPos.X;
                var deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * camera.sensitivity;
                camera.Pitch -= deltaY * camera.sensitivity;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }
            base.OnMouseMove(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferObject);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayObject);
            GL.UseProgram(0);
            GL.DeleteShader(shader.Handle);
            GL.DeleteTexture(texture.Handle);
            shader.Dispose();
            base.OnUnload(e);
        }
    }
}
