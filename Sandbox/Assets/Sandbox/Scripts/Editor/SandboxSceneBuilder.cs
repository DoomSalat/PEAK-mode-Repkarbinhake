using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Repkarbinhake.Sandbox.Editor
{
    public static class SandboxSceneBuilder // Static: editor utility invoked from a menu item
    {
        private const string Root = "Assets/Sandbox";
        private const string GeneratedFolder = Root + "/Generated";
        private const string ScenePath = Root + "/Sandbox.unity";
        private const string IconPath = Root + "/Icons/RepkarbinReticle.png";
        private const string LockClipPath = Root + "/Audio/padlock_key_lock_03.mp3";
        private const string UnlockClipPath = Root + "/Audio/padlock_key_unlock_04.mp3";
        private const string MixerPath = Root + "/Audio/SandboxMixer.mixer";
        private const string SfxGroupName = "SFX";
        private const string MixerControllerType = "UnityEditor.Audio.AudioMixerController";
        private const string StandardShader = "Standard";
        private const int ClimberCount = 4;
        private const int ClimberLayer = 2;
        private const float ClimberSpacing = 1.6f;
        private const string RuggedSlopeMeshPath = GeneratedFolder + "/RuggedSlope.asset";
        private const int RuggedSlopeCells = 24;
        private const float RuggedSlopeWidth = 10f;
        private const float RuggedSlopeDepth = 10f;
        private const float RuggedSlopeRise = 12f;
        private const float RuggedSlopeBump = 0.45f;
        private const float RuggedSlopeNoiseScale = 0.6f;

        [MenuItem("Repkarbinhake/Build Sandbox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder(GeneratedFolder);
            PrepareIcon();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            SandboxRopeSettings settings = LoadOrCreate<SandboxRopeSettings>(GeneratedFolder + "/SandboxRopeSettings.asset");
            Material ground = LoadOrCreateMaterial("Ground", new Color(0.42f, 0.45f, 0.4f));
            Material rock = LoadOrCreateMaterial("Rock", new Color(0.5f, 0.47f, 0.44f));
            Material climberMaterial = LoadOrCreateMaterial("Climber", Color.white);
            Material ropeMaterial = LoadOrCreateMaterial("Rope", new Color(0.55f, 0.36f, 0.18f));
            ropeMaterial.EnableKeyword("_EMISSION");
            PhysicsMaterial grip = LoadOrCreatePhysicsMaterial("ClimberGrip", 0.9f, 1f, PhysicsMaterialCombine.Maximum);
            PhysicsMaterial slide = LoadOrCreatePhysicsMaterial("ClimberSlide", 0.15f, 0.15f, PhysicsMaterialCombine.Minimum);

            BuildMountain(ground, rock);
            SandboxClimber[] climbers = BuildClimbers(climberMaterial, grip, slide);
            SandboxCamera sandboxCamera = BuildCamera();
            BuildBack(settings, climbers, sandboxCamera, ropeMaterial);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"{nameof(SandboxSceneBuilder)}: scene saved to {ScenePath}");
        }

        private static void BuildMountain(Material ground, Material rock)
        {
            Block("Ground", new Vector3(0f, -0.5f, 0f), new Vector3(60f, 1f, 60f), Vector3.zero, ground);
            Block("Slope 25", new Vector3(0f, 1.6f, 8f), new Vector3(10f, 0.6f, 10f), new Vector3(-25f, 0f, 0f), ground);
            Block("Ledge", new Vector3(0f, 3.75f, 15.5f), new Vector3(10f, 1f, 6f), Vector3.zero, ground);
            Block("Slope 45", new Vector3(0f, 6.6f, 21f), new Vector3(10f, 0.6f, 8f), new Vector3(-45f, 0f, 0f), ground);
            Block("Top", new Vector3(0f, 9.4f, 27.5f), new Vector3(10f, 1f, 8f), Vector3.zero, ground);
            Block("Cliff", new Vector3(8.5f, 5f, 12f), new Vector3(1f, 10f, 10f), Vector3.zero, rock);
            Block("Cliff ledge", new Vector3(10.5f, 9.5f, 12f), new Vector3(3f, 1f, 10f), Vector3.zero, rock);

            Rock(new Vector3(-2f, 0.5f, 4f), 1.2f, rock);
            Rock(new Vector3(2.5f, 1.3f, 7f), 1f, rock);
            Rock(new Vector3(-3f, 4.6f, 15f), 1.4f, rock);
            Rock(new Vector3(1.5f, 4.4f, 16.5f), 0.9f, rock);
            Rock(new Vector3(3f, 0.6f, -4f), 1.5f, rock);
            Rock(new Vector3(-6f, 0.7f, -2f), 1.8f, rock);
            RuggedSlope(new Vector3(-20f, 0f, 2f), rock);
        }

        private static SandboxClimber[] BuildClimbers(Material material, PhysicsMaterial grip, PhysicsMaterial slide)
        {
            SandboxClimber[] climbers = new SandboxClimber[ClimberCount];

            for (int i = 0; i < ClimberCount; i++)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = $"Climber #{i + 1}";
                body.layer = ClimberLayer;
                body.transform.position = new Vector3((i - (ClimberCount - 1) / 2f) * ClimberSpacing, 1.1f, -2f);
                body.GetComponent<MeshRenderer>().sharedMaterial = material;

                CapsuleCollider capsule = body.GetComponent<CapsuleCollider>();
                capsule.sharedMaterial = grip;

                Rigidbody rigidbody = body.AddComponent<Rigidbody>();
                rigidbody.mass = 1f;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rigidbody.angularDamping = 1f;

                Transform ropePoint = new GameObject("RopePoint").transform;
                ropePoint.SetParent(body.transform, false);
                ropePoint.localPosition = new Vector3(0f, 0.35f, 0f);

                GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nose.name = "Nose";
                Object.DestroyImmediate(nose.GetComponent<BoxCollider>());
                nose.transform.SetParent(body.transform, false);
                nose.transform.localPosition = new Vector3(0f, 0.5f, 0.45f);
                nose.transform.localScale = new Vector3(0.25f, 0.12f, 0.2f);

                SandboxClimber climber = body.AddComponent<SandboxClimber>();
                SerializedObject serialized = new SerializedObject(climber);
                serialized.FindProperty("_body").objectReferenceValue = rigidbody;
                serialized.FindProperty("_capsule").objectReferenceValue = capsule;
                serialized.FindProperty("_ropePoint").objectReferenceValue = ropePoint;
                serialized.FindProperty("_bodyRenderer").objectReferenceValue = body.GetComponent<MeshRenderer>();
                serialized.FindProperty("_gripMaterial").objectReferenceValue = grip;
                serialized.FindProperty("_slideMaterial").objectReferenceValue = slide;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                climbers[i] = climber;
            }

            return climbers;
        }

        private static SandboxCamera BuildCamera()
        {
            Camera camera = Camera.main;
            SandboxCamera sandboxCamera = camera.gameObject.AddComponent<SandboxCamera>();
            SerializedObject serialized = new SerializedObject(sandboxCamera);
            serialized.FindProperty("_camera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return sandboxCamera;
        }

        private static void BuildBack(SandboxRopeSettings settings, SandboxClimber[] climbers, SandboxCamera sandboxCamera, Material ropeMaterial)
        {
            GameObject backObject = new GameObject("Sandbox Rope Back");
            SandboxDebugOverlay overlay = backObject.AddComponent<SandboxDebugOverlay>();
            SerializedObject overlaySerialized = new SerializedObject(overlay);
            overlaySerialized.FindProperty("_reticleIcon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            overlaySerialized.ApplyModifiedPropertiesWithoutUndo();

            AudioSource source = backObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0.6f;
            source.outputAudioMixerGroup = EnsureSfxMixerGroup();
            CarabinerAudio audio = backObject.AddComponent<CarabinerAudio>();
            SerializedObject audioSerialized = new SerializedObject(audio);
            audioSerialized.FindProperty("_source").objectReferenceValue = source;
            audioSerialized.FindProperty("_lockClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(LockClipPath);
            audioSerialized.FindProperty("_unlockClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(UnlockClipPath);
            audioSerialized.ApplyModifiedPropertiesWithoutUndo();

            SandboxRopeBack back = backObject.AddComponent<SandboxRopeBack>();
            SerializedObject backSerialized = new SerializedObject(back);
            backSerialized.FindProperty("_settings").objectReferenceValue = settings;
            backSerialized.FindProperty("_camera").objectReferenceValue = sandboxCamera;
            backSerialized.FindProperty("_overlay").objectReferenceValue = overlay;
            backSerialized.FindProperty("_audio").objectReferenceValue = audio;
            backSerialized.FindProperty("_ropeMaterial").objectReferenceValue = ropeMaterial;
            backSerialized.FindProperty("_groundMask").intValue = 1 << LayerMask.NameToLayer("Default");
            backSerialized.FindProperty("_cliffHolderSpot").objectReferenceValue = Spot("Cliff Holder Spot", new Vector3(11.2f, 10f, 12f));
            backSerialized.FindProperty("_cliffEdgeSpot").objectReferenceValue = Spot("Cliff Edge Spot", new Vector3(8.4f, 10f, 12f));
            SerializedProperty climbersProperty = backSerialized.FindProperty("_climbers");
            climbersProperty.arraySize = climbers.Length;

            for (int i = 0; i < climbers.Length; i++)
                climbersProperty.GetArrayElementAtIndex(i).objectReferenceValue = climbers[i];

            backSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RuggedSlope(Vector3 origin, Material material)
        {
            int row = RuggedSlopeCells + 1;
            Vector3[] vertices = new Vector3[row * row];
            int[] triangles = new int[RuggedSlopeCells * RuggedSlopeCells * 6];

            for (int z = 0; z < row; z++)
            {
                for (int x = 0; x < row; x++)
                {
                    float u = (float)x / RuggedSlopeCells;
                    float v = (float)z / RuggedSlopeCells;
                    float bump = (Mathf.PerlinNoise(u * RuggedSlopeWidth * RuggedSlopeNoiseScale, v * RuggedSlopeDepth * RuggedSlopeNoiseScale) - 0.5f) * 2f * RuggedSlopeBump;
                    vertices[z * row + x] = new Vector3(u * RuggedSlopeWidth, v * RuggedSlopeRise + bump, v * RuggedSlopeDepth);
                }
            }

            int index = 0;

            for (int z = 0; z < RuggedSlopeCells; z++)
            {
                for (int x = 0; x < RuggedSlopeCells; x++)
                {
                    int corner = z * row + x;
                    triangles[index++] = corner;
                    triangles[index++] = corner + row;
                    triangles[index++] = corner + 1;
                    triangles[index++] = corner + 1;
                    triangles[index++] = corner + row;
                    triangles[index++] = corner + row + 1;
                }
            }

            Mesh mesh = new Mesh { name = "Rugged Slope", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.DeleteAsset(RuggedSlopeMeshPath);
            AssetDatabase.CreateAsset(mesh, RuggedSlopeMeshPath);

            GameObject slope = new GameObject("Rugged slope (mesh)");
            slope.transform.position = origin;
            slope.AddComponent<MeshFilter>().sharedMesh = mesh;
            slope.AddComponent<MeshRenderer>().sharedMaterial = material;
            slope.AddComponent<MeshCollider>().sharedMesh = mesh;
            slope.isStatic = true;
        }

        private static void Block(string name, Vector3 position, Vector3 scale, Vector3 euler, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            block.transform.localScale = scale;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
            block.isStatic = true;
        }

        private static Transform Spot(string name, Vector3 position)
        {
            Transform spot = new GameObject(name).transform;
            spot.position = position;
            return spot;
        }

        private static void Rock(Vector3 position, float size, Material material)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(size, size * 0.8f, size);
            rock.GetComponent<MeshRenderer>().sharedMaterial = material;
            rock.isStatic = true;
        }

        private static void PrepareIcon()
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{GeneratedFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null)
                return material;

            material = new Material(Shader.Find(StandardShader)) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static PhysicsMaterial LoadOrCreatePhysicsMaterial(string name, float dynamicFriction, float staticFriction, PhysicsMaterialCombine combine)
        {
            string path = $"{GeneratedFolder}/{name}.physicMaterial";
            PhysicsMaterial material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);

            if (material != null)
                return material;

            material = new PhysicsMaterial(name)
            {
                dynamicFriction = dynamicFriction,
                staticFriction = staticFriction,
                frictionCombine = combine,
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static AudioMixerGroup EnsureSfxMixerGroup()
        {
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            System.Type controllerType = typeof(UnityEditor.Editor).Assembly.GetType(MixerControllerType);

            if (mixer == null)
                mixer = (AudioMixer)controllerType.GetMethod("CreateMixerControllerAtPath").Invoke(null, new object[] { MixerPath });

            AudioMixerGroup[] existing = mixer.FindMatchingGroups(SfxGroupName);

            if (existing.Length > 0)
                return existing[0];

            // Unity has no public API to add mixer groups; AudioMixerController is the editor-internal type behind AudioMixer assets.
            object group = controllerType.GetMethod("CreateNewGroup").Invoke(mixer, new object[] { SfxGroupName, false });
            object master = controllerType.GetProperty("masterGroup").GetValue(mixer);
            controllerType.GetMethod("AddChildToParent").Invoke(mixer, new[] { group, master });
            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();
            return (AudioMixerGroup)group;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(Root, "Generated");
        }
    }
}
