using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[InitializeOnLoad]
public static class HierarchyHighlighter
{
    [Serializable]
    public class Rule
    {
        public string key;
        public string tag;
        public Color color = Color.white;
        public Texture2D icon;
        public bool enabled = true;
    }

    private static List<Rule> rules = new List<Rule>();

    private const string PREF_KEY = "HierarchyHighlighterRules";

    static HierarchyHighlighter()
    {
        LoadRules();
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        string nameLower = go.name.ToLower();
        Rule match = null;
        foreach (var r in rules)
        {
            if (!r.enabled) continue;
            if (nameLower.Contains(r.key.ToLower())) { match = r; break; }
        }
        if (match == null) return;

        bool isSelected = Array.IndexOf(Selection.instanceIDs, instanceID) >= 0;
        Rect contentRect = selectionRect;

        Color bg = isSelected 
            ? new Color(0.24f, 0.48f, 0.90f, 1f) 
            : new Color(0.13f, 0.13f, 0.13f, 1f); 
        EditorGUI.DrawRect(selectionRect, bg);

        const float iconSize = 16f, iconPadding = 2f;
        if (match.icon != null)
        {
            Rect iconRect = new Rect(selectionRect.x + iconPadding,
                selectionRect.y + (selectionRect.height - iconSize) / 2f,
                iconSize, iconSize);
            GUI.DrawTexture(iconRect, match.icon, ScaleMode.ScaleToFit);
            contentRect.x += iconSize + iconPadding;
            contentRect.width -= iconSize + iconPadding;
        }

        float pillW = 60f, pillH = 14f;
        Rect pillRect = new Rect(selectionRect.xMax - pillW - 6f,
            selectionRect.y + (selectionRect.height - pillH) / 2f,
            pillW, pillH);
        EditorGUI.DrawRect(pillRect, match.color);
        GUIStyle pillStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        float brightness = match.color.r * 0.299f + match.color.g * 0.587f + match.color.b * 0.114f;
        pillStyle.normal.textColor = brightness > 0.6f ? Color.black : Color.white;
        GUI.Label(pillRect, match.tag, pillStyle);
        contentRect.width -= (pillW + 8f);

        GUIStyle style = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
        style.normal.textColor = isSelected ? Color.white : match.color;
        GUI.Label(contentRect, go.name, style);
    }



    private static string RulesPath => "ProjectSettings/HierarchyHighlighter.json";

    private static void SaveRules()
    {
        string json = JsonUtility.ToJson(new Wrapper() { items = rules }, true);
        System.IO.File.WriteAllText(RulesPath, json);
        AssetDatabase.Refresh();
    }

    private static void LoadRules()
    {
        if (!System.IO.File.Exists(RulesPath))
        {
            SetDefaultRules();
            SaveRules();
            return;
        }
        string json = System.IO.File.ReadAllText(RulesPath);
        rules = JsonUtility.FromJson<Wrapper>(json).items;
}

    private static void SetDefaultRules()
    {
        rules = new List<Rule>()
        {
            // === LIGHTING & VISUAL ===
            new Rule(){ key="light", tag="Light", color=new Color(1f,0.85f,0.2f), icon=EditorGUIUtility.IconContent("d_Light Icon").image as Texture2D },
            new Rule(){ key="directional", tag="Directional Light", color=new Color(1f,0.95f,0.4f), icon=EditorGUIUtility.IconContent("d_Light Icon").image as Texture2D },
            new Rule(){ key="spotlight", tag="Spotlight", color=new Color(1f,0.8f,0.3f), icon=EditorGUIUtility.IconContent("d_Light Icon").image as Texture2D },
            new Rule(){ key="pointlight", tag="Point Light", color=new Color(1f,0.7f,0.4f), icon=EditorGUIUtility.IconContent("d_Light Icon").image as Texture2D },
            new Rule(){ key="reflection", tag="Reflection Probe", color=new Color(0.4f,0.9f,1f), icon=EditorGUIUtility.IconContent("d_ReflectionProbe Icon").image as Texture2D },
            new Rule(){ key="sky", tag="Skybox", color=new Color(0.3f,0.7f,1f), icon=EditorGUIUtility.IconContent("Skybox Icon").image as Texture2D },

            // === AUDIO ===
            new Rule(){ key="audiosource", tag="Audio", color=new Color(0.3f,0.7f,1f), icon=EditorGUIUtility.IconContent("d_AudioSource Icon").image as Texture2D },
            new Rule(){ key="music", tag="Music", color=new Color(0.5f,0.8f,1f), icon=EditorGUIUtility.IconContent("d_AudioClip Icon").image as Texture2D },
            new Rule(){ key="sfx", tag="SFX", color=new Color(0.6f,0.9f,1f), icon=EditorGUIUtility.IconContent("d_AudioSource Icon").image as Texture2D },
            new Rule(){ key="voice", tag="Voice", color=new Color(0.4f,0.8f,1f), icon=EditorGUIUtility.IconContent("d_AudioSource Icon").image as Texture2D },

            // === CAMERA ===
            new Rule(){ key="camera", tag="Camera", color=new Color(0.4f,0.8f,1f), icon=EditorGUIUtility.IconContent("Camera Icon").image as Texture2D },
            new Rule(){ key="cinemachine", tag="Cinemachine", color=new Color(0.6f,0.9f,1f), icon=EditorGUIUtility.IconContent("d_Camera Icon").image as Texture2D },
            new Rule(){ key="cutscene", tag="Cutscene", color=new Color(0.8f,0.5f,1f), icon=EditorGUIUtility.IconContent("TimelineAsset Icon").image as Texture2D },

            // === FX ===
            new Rule(){ key="particle", tag="FX", color=new Color(1f,0.5f,1f), icon=EditorGUIUtility.IconContent("ParticleSystem Icon").image as Texture2D },
            new Rule(){ key="explosion", tag="Explosion", color=new Color(1f,0.3f,0.3f), icon=EditorGUIUtility.IconContent("ParticleSystem Icon").image as Texture2D },
            new Rule(){ key="fire", tag="Fire", color=new Color(1f,0.4f,0.1f), icon=EditorGUIUtility.IconContent("ParticleSystem Icon").image as Texture2D },
            new Rule(){ key="smoke", tag="Smoke", color=new Color(0.6f,0.6f,0.6f), icon=EditorGUIUtility.IconContent("ParticleSystem Icon").image as Texture2D },
            new Rule(){ key="water", tag="Water", color=new Color(0.2f,0.6f,1f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },

            // === UI ===
            new Rule(){ key="canvas", tag="UI", color=new Color(0.9f,0.7f,1f), icon=EditorGUIUtility.IconContent("Canvas Icon").image as Texture2D },
            new Rule(){ key="ui", tag="UI", color=new Color(0.9f,0.7f,1f), icon=EditorGUIUtility.IconContent("Canvas Icon").image as Texture2D },
            new Rule(){ key="button", tag="Button", color=new Color(0.6f,0.8f,1f), icon=EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image as Texture2D },
            new Rule(){ key="panel", tag="Panel", color=new Color(0.8f,0.8f,1f), icon=EditorGUIUtility.IconContent("d_UnityEditor.GameView").image as Texture2D },
            new Rule(){ key="hud", tag="HUD", color=new Color(0.8f,1f,0.6f), icon=EditorGUIUtility.IconContent("Canvas Icon").image as Texture2D },

            // === CHARACTERS ===
            new Rule(){ key="char", tag="Character", color=new Color(0.3f,1f,0.3f), icon=EditorGUIUtility.IconContent("d_Prefab Icon").image as Texture2D },
            new Rule(){ key="npc", tag="NPC", color=new Color(0.2f,0.9f,0.5f), icon=EditorGUIUtility.IconContent("Avatar Icon").image as Texture2D },
            new Rule(){ key="enemy", tag="Enemy", color=new Color(1f,0.3f,0.3f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="boss", tag="Boss", color=new Color(0.7f,0.2f,0.2f), icon=EditorGUIUtility.IconContent("PrefabVariant Icon").image as Texture2D },
            new Rule(){ key="player", tag="Player", color=new Color(0.2f,0.8f,1f), icon=EditorGUIUtility.IconContent("d_Avatar Icon").image as Texture2D },
            new Rule(){ key="weapon", tag="Weapon", color=new Color(0.9f,0.6f,0.2f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="projectile", tag="Projectile", color=new Color(1f,0.4f,0.2f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="collectible", tag="Collectible", color=new Color(1f,1f,0.3f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="checkpoint", tag="Checkpoint", color=new Color(0.4f,1f,0.4f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },

            // === PHYSICS ===
            new Rule(){ key="trigger", tag="Trigger", color=new Color(1f,0.4f,0.4f), icon=EditorGUIUtility.IconContent("d_Collider2D Icon").image as Texture2D },
            new Rule(){ key="collider", tag="Collider", color=new Color(1f,0.6f,0.2f), icon=EditorGUIUtility.IconContent("BoxCollider Icon").image as Texture2D },
            new Rule(){ key="rigidbody", tag="Rigidbody", color=new Color(0.4f,0.9f,0.9f), icon=EditorGUIUtility.IconContent("Rigidbody Icon").image as Texture2D },
            new Rule(){ key="physics", tag="Physics", color=new Color(0.3f,0.8f,0.8f), icon=EditorGUIUtility.IconContent("PhysicsMaterial Icon").image as Texture2D },

            // === ENVIRONMENT ===
            new Rule(){ key="terrain", tag="Terrain", color=new Color(0.5f,1f,0.5f), icon=EditorGUIUtility.IconContent("Terrain Icon").image as Texture2D },
            new Rule(){ key="tree", tag="Tree", color=new Color(0.3f,0.8f,0.3f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="rock", tag="Rock", color=new Color(0.6f,0.6f,0.6f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="building", tag="Building", color=new Color(0.7f,0.5f,0.4f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="door", tag="Door", color=new Color(0.8f,0.5f,0.2f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="level", tag="Level", color=new Color(0.2f,0.6f,1f), icon=EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D },
            new Rule(){ key="spawn", tag="Spawn", color=new Color(0.5f,1f,0.5f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },

            // === SYSTEM ===
            new Rule(){ key="manager", tag="Manager", color=new Color(0.9f,0.9f,0.5f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="system", tag="System", color=new Color(0.9f,0.9f,0.9f), icon=EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D },
            new Rule(){ key="game", tag="Game", color=new Color(0.7f,0.7f,1f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="network", tag="Network", color=new Color(0.3f,0.8f,1f), icon=EditorGUIUtility.IconContent("NetworkManager Icon").image as Texture2D },
            new Rule(){ key="navmesh", tag="NavMesh", color=new Color(0.5f,0.9f,0.9f), icon=EditorGUIUtility.IconContent("d_NavMeshData Icon").image as Texture2D },
            new Rule(){ key="ai", tag="AI", color=new Color(0.8f,0.4f,1f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="path", tag="Path", color=new Color(0.4f,0.9f,0.4f), icon=EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D },
            new Rule(){ key="script", tag="Script", color=new Color(1f,1f,1f), icon=EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D },
            new Rule(){ key="shader", tag="Shader", color=new Color(0.4f,0.6f,1f), icon=EditorGUIUtility.IconContent("Shader Icon").image as Texture2D },

            // === ANIMATION ===
            new Rule(){ key="animator", tag="Animator", color=new Color(1f,0.6f,0.8f), icon=EditorGUIUtility.IconContent("Animator Icon").image as Texture2D },
            new Rule(){ key="animation", tag="Animation", color=new Color(1f,0.7f,0.9f), icon=EditorGUIUtility.IconContent("Animation Icon").image as Texture2D },
            new Rule(){ key="timeline", tag="Timeline", color=new Color(0.8f,0.5f,1f), icon=EditorGUIUtility.IconContent("TimelineAsset Icon").image as Texture2D },

            // === DEBUG / TOOLS ===
            new Rule(){ key="debug", tag="Debug", color=new Color(1f,0.3f,0.6f), icon=EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image as Texture2D },
            new Rule(){ key="tool", tag="Tool", color=new Color(0.7f,0.9f,1f), icon=EditorGUIUtility.IconContent("EditorSettings Icon").image as Texture2D },
            new Rule(){ key="gizmo", tag="Gizmo", color=new Color(0.4f,0.8f,0.4f), icon=EditorGUIUtility.IconContent("SceneViewTools Icon").image as Texture2D },
        };
    }


    [Serializable]
    private class Wrapper { public List<Rule> items; }

    public class HierarchyHighlighterWindow : EditorWindow
    {
        private Vector2 scroll;

        [MenuItem("Tools/Hierarchy Highlighter")]
        public static void OpenWindow()
        {
            GetWindow<HierarchyHighlighterWindow>("Hierarchy Highlighter");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Hierarchy Highlighter Rules", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < rules.Count; i++)
            {
                var r = rules[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                r.enabled = EditorGUILayout.Toggle(r.enabled, GUILayout.Width(20));
                r.key = EditorGUILayout.TextField("Key", r.key);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    rules.RemoveAt(i);
                    SaveRules();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                r.tag = EditorGUILayout.TextField("Tag", r.tag);
                r.color = EditorGUILayout.ColorField("Color", r.color);
                r.icon = (Texture2D)EditorGUILayout.ObjectField("Icon", r.icon, typeof(Texture2D), false);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Rule")) rules.Add(new Rule() { key = "new", tag = "New", color = Color.white });

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset to Defaults")) { SetDefaultRules(); SaveRules(); }
            if (GUILayout.Button("Save Rules")) SaveRules();
        }
    }
}
