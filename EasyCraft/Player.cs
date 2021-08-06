using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using WIN = System.Windows.Forms;
using EasyCraft.engine;
using EasyCraft.engine.extensions;

namespace EasyCraft
{
    public class Player : Behavior
    {
        public bool isGrounded;
        public bool isSprinting;

        public float mouseSensitivity = 14f;

        public float walkSpeed = 6f;
        public float sprintSpeed = 12f;
        public float jumpForce = 15f;
        public float freeCamSpeed = 40f;
        public float zoomSensitivity = 10f;
        public float fastZoomSensitivity = 50f;
        public float gravity = -57.46f;

        public float playerWidth = 0.15f;
        public float boundsTolerance = 0.1f;

        public PauseScreen pauseScreen;
        public bool paused { get => pauseScreen.active; set => pauseScreen.active = value; }

        private float horizontal;
        private float vertical;
        private float mouseHorizontal;
        private float mouseVertical;
        private Vector3 velocity;
        private float verticalMomentum = 0;
        private bool jumpRequest;
        private Transform camera;
        private bool freeCam = false;

        private MeshRenderer debugRenderer;

        public void Reload()
        {
            log("Reload");
            dispose(debugRenderer.material); 
            debugRenderer.material = new Material();
            debugRenderer.material.shader = Shader.FromFile("shaders/error.hlsl");
            debugRenderer.material.textures.Add(ShaderTexture2D.FromFile("easycraft/textures/blocks/stone.png"));
        }

        public override void Start()
        {
            camera = Camera.main.transform;
            camera.localPosition = new Vector3(0, 1.8f, 0);

            debugRenderer = new MeshRenderer() { name = "Player Renderer" };
            debugRenderer.active = false;
            debugRenderer.transform.SetParent(transform);

            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[] {
                // Back
                new Vector3(-0.5f,0,-0.5f),
                new Vector3(-0.5f,2f,-0.5f),
                new Vector3(0.5f,2f,-0.5f),
                new Vector3(0.5f,0,-0.5f),

                // Front
                new Vector3(-0.5f,0,0.5f),
                new Vector3(-0.5f,2f,0.5f),
                new Vector3(0.5f,2f,0.5f),
                new Vector3(0.5f,0,0.5f),
                
                // Left
                new Vector3(-0.5f,0,0.5f),
                new Vector3(-0.5f,2f,0.5f),
                new Vector3(-0.5f,2f,-0.5f),
                new Vector3(-0.5f,0,-0.5f),

                // Right
                new Vector3(0.5f,0,0.5f),
                new Vector3(0.5f,2f,0.5f),
                new Vector3(0.5f,2f,-0.5f),
                new Vector3(0.5f,0,-0.5f),

                // Top
                new Vector3(-0.5f,2f,0.5f),
                new Vector3(0.5f,2f,0.5f),
                new Vector3(0.5f,2f,-0.5f),
                new Vector3(-0.5f,2f,-0.5f),

                // Bottom
                new Vector3(-0.5f,0,0.5f),
                new Vector3(0.5f,0,0.5f),
                new Vector3(0.5f,0,-0.5f),
                new Vector3(-0.5f,0,-0.5f)
            };

            mesh.normals = new Vector3[]
            {
                // Back
                new Vector3(0,0,-1),
                new Vector3(0,0,-1),
                new Vector3(0,0,-1),
                new Vector3(0,0,-1),

                // Front
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,1),

                // Left
                new Vector3(-1,0,0),
                new Vector3(-1,0,0),
                new Vector3(-1,0,0),
                new Vector3(-1,0,0),

                // Right
                new Vector3(1,0,0),
                new Vector3(1,0,0),
                new Vector3(1,0,0),
                new Vector3(1,0,0),

                // Top
                new Vector3(0,1,0),
                new Vector3(0,1,0),
                new Vector3(0,1,0),
                new Vector3(0,1,0),

                // Bottom
                new Vector3(0,-1,0),
                new Vector3(0,-1,0),
                new Vector3(0,-1,0),
                new Vector3(0,-1,0)
            };

            mesh.uv = new Vector2[] {
                // Back
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),

                // Front
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),

                // Left
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),

                // Right
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),

                // Top
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),

                // Bottom
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),
            };

            mesh.triangles = new int[] {
                0,1,2,0,2,3,
                6,5,4,7,6,4,
                8,9,10,8,10,11,
                14,13,12,15,14,12,
                16,17,18,16,18,19,
                22,21,20,23,22,20
            };

            debugRenderer.mesh = mesh;
            Reload();
        }

        public override void FixedUpdate()
        {
            if (freeCam)
            {
                float movementSpeed = isSprinting ? freeCamSpeed * 2f : freeCamSpeed;
                if (Input.GetKey(WIN.Keys.A) || Input.GetKey(WIN.Keys.Left))
                {
                    camera.position += -camera.right * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.D) || Input.GetKey(WIN.Keys.Right))
                {
                    camera.position += camera.right * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.W) || Input.GetKey(WIN.Keys.Up))
                {
                    camera.position += camera.forward * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.S) || Input.GetKey(WIN.Keys.Down))
                {
                    camera.position += -camera.forward * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.Q))
                {
                    camera.position += camera.up * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.E))
                {
                    camera.position += -camera.up * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.R) || Input.GetKey(WIN.Keys.PageUp))
                {
                    camera.position += Vector3.Up * movementSpeed * Time.fixedDeltaTime;
                }

                if (Input.GetKey(WIN.Keys.F) || Input.GetKey(WIN.Keys.PageDown))
                {
                    camera.position += -Vector3.Up * movementSpeed * Time.fixedDeltaTime;
                }

                Vector3 euler = camera.localRotation.ToEuler();
                float newRotationX = euler.X + Input.GetAxis("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;
                float newRotationY = euler.Y + Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
                camera.localRotation = QuaternionExtensions.FromEulerAngles(newRotationX, newRotationY, 0);
                float axis = Input.GetAxis("Mouse ScrollWheel");
                if (axis != 0)
                {
                    var zs = isSprinting ? fastZoomSensitivity : zoomSensitivity;
                    camera.position += camera.forward * axis * zs;
                }
            }
            else
            {
                if (!World.Instance.WorldLoaded || paused)
                    return;
                CalculateVelocity();
                if (jumpRequest)
                    Jump();

                transform.Rotate(Vector3.Up * mouseHorizontal * mouseSensitivity * Time.fixedDeltaTime);
                camera.Rotate(Vector3.Right * mouseVertical * mouseSensitivity * Time.fixedDeltaTime);
                transform.Translate(velocity, Space.World);
            }
        }

        public override void Update()
        {
            GetPlayerInputs();

            if (World.Instance.WorldLoaded && Input.GetKeyDown(WIN.Keys.Escape))
            {
                paused = !paused;
                Input.CursorMode = paused ? CursorMode.Normal : CursorMode.Lock;
                Input.ShowCursor = paused;
            }
        }

        void Jump()
        {
            verticalMomentum = jumpForce;
            isGrounded = false;
            jumpRequest = false;
        }

        private void CalculateVelocity()
        {
            // Affect vertical momentum with gravity.
            if (verticalMomentum > gravity)
                verticalMomentum += Time.fixedDeltaTime * gravity;

            // if we're sprinting, use the sprint multiplier.
            if (isSprinting)
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
            else
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

            // Apply vertical momentum (falling/jumping).
            velocity += Vector3.Up * verticalMomentum * Time.fixedDeltaTime;

            if ((velocity.Z > 0 && front) || (velocity.Z < 0 && back))
                velocity.Z = 0;
            if ((velocity.X > 0 && right) || (velocity.X < 0 && left))
                velocity.X = 0;

            if (velocity.Y < 0)
                velocity.Y = checkDownSpeed(velocity.Y);
            else if (velocity.Y > 0)
                velocity.Y = checkUpSpeed(velocity.Y);
        }

        private void GetPlayerInputs()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            mouseHorizontal = Input.GetAxis("Mouse X");
            mouseVertical = Input.GetAxis("Mouse Y");

            isSprinting = Input.GetKey(WIN.Keys.ControlKey);

            if (isGrounded && Input.GetKey(WIN.Keys.Space))
                jumpRequest = true;

            if (Input.GetKeyDown(WIN.Keys.U))
            {
                freeCam = !freeCam;
                debugRenderer.active = freeCam;
                if (!freeCam)
                {
                    camera.localPosition = new Vector3(0, 1.8f, 0);
                    camera.localRotation = Quaternion.Identity;
                }
            }
        }

        private float checkDownSpeed(float downSpeed)
        {

            if (
                World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y + downSpeed, transform.position.Z - playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y + downSpeed, transform.position.Z - playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y + downSpeed, transform.position.Z + playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y + downSpeed, transform.position.Z + playerWidth)
               )
            {

                isGrounded = true;
                return 0;

            }
            else
            {

                isGrounded = false;
                return downSpeed;

            }

        }

        private float checkUpSpeed(float upSpeed)
        {

            if (
                World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y + 2f + upSpeed, transform.position.Z - playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y + 2f + upSpeed, transform.position.Z - playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y + 2f + upSpeed, transform.position.Z + playerWidth) ||
                World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y + 2f + upSpeed, transform.position.Z + playerWidth)
               )
            {

                return 0;

            }
            else
            {

                return upSpeed;

            }

        }

        public bool front
        {

            get
            {
                if (
                    World.Instance.CheckForVoxel(transform.position.X, transform.position.Y, transform.position.Z + playerWidth) ||
                    World.Instance.CheckForVoxel(transform.position.X, transform.position.Y + 1f, transform.position.Z + playerWidth)
                    )
                    return true;
                else
                    return false;
            }

        }
        public bool back
        {

            get
            {
                if (
                    World.Instance.CheckForVoxel(transform.position.X, transform.position.Y, transform.position.Z - playerWidth) ||
                    World.Instance.CheckForVoxel(transform.position.X, transform.position.Y + 1f, transform.position.Z - playerWidth)
                    )
                    return true;
                else
                    return false;
            }

        }
        public bool left
        {

            get
            {
                if (
                    World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y, transform.position.Z) ||
                    World.Instance.CheckForVoxel(transform.position.X - playerWidth, transform.position.Y + 1f, transform.position.Z)
                    )
                    return true;
                else
                    return false;
            }

        }
        public bool right
        {

            get
            {
                if (
                    World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y, transform.position.Z) ||
                    World.Instance.CheckForVoxel(transform.position.X + playerWidth, transform.position.Y + 1f, transform.position.Z)
                    )
                    return true;
                else
                    return false;
            }

        }
    }
}