// ====================================================================================================================
// DiaQ. Dialogue and Quest Engine for Unity
// www.plyoung.com
// Copyright (c) 2013 by Leslie Young
// ====================================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using DiaQ;

#if UNIRPG_CORE
using UniRPG;
using UniRPGEditor;
#endif

namespace DiaQEditor
{
	public class DiaQEditorWindow : EditorWindow
	{
		// ============================================================================================================
		#region vars

		private static string assetFile = "DiaQ.asset";

#if UNIRPG_CORE
		private static string assetsLocation = "Assets/UniRPG Data/Resources/";
#else
		private static string assetsLocation = "Assets/DiaQ/Sample/Resources/";
#endif

		private static DiaQAsset asset = null;

		private static DiaQGraph currGraph { get { return _currGraph; } set { _currGraph = value; inspectorScroll[1] = Vector2.zero; linkNode = null; _currQuest = null; } }
		private static DiaQGraph _currGraph = null;

		private static DiaQNode currNode { get { return _currNode; } set { _currNode = value; inspectorScroll[1] = Vector2.zero; linkNode = null; _currQuest = null; } }
		private static DiaQNode _currNode = null;

		private static DiaQuest currQuest { get { return _currQuest; } set { _currQuest = value; inspectorScroll[1] = Vector2.zero; linkNode = null; _currGraph = null; _currNode = null; } }
		private static DiaQuest _currQuest = null;

		private static DiaQNode linkNode = null;
		private static int linkOutSlot = 0;
		private static Vector2 linkOutPos1 = Vector2.zero;

		// ------------------------------------------------------------------------------------------------------------

		private static int inspectorArea = 0; // selected option in inspector toolbar
		private static bool showComments = true;

		private static bool[] draggingSplitter = { false, false };
		private static bool draggingGraph = false;

		private static float[] splitterPos = { 0f, 0f };
		private static float inspectorWidthPercentage;
		private static Vector2 prevScreenSize = Vector2.zero;

		private static Rect mainRect;
		private static Rect inspectorRect;
		private static Rect[] splitterRect = new Rect[2];
		private static Rect questRect = new Rect(0, 10f, 400f, 400f);

		private const float inspectorMinWidth = 300f;	// min width of inspector
		private const float inspectorMinHeight = 100f;	// min height of the horizontal splitter of inspector
		private static Vector2[] inspectorScroll = { Vector2.zero, Vector2.zero };

		#endregion
		// ============================================================================================================
		#region sys

#if UNIRPG_CORE
		[MenuItem("UniRPG/DiaQ Editor", priority = 4)]
#endif
		[MenuItem("Window/DiaQ Editor", priority = 0)]
		public static void ShowDiaQEditor()
		{
			DiaQEditorWindow window = EditorWindow.GetWindow<DiaQEditorWindow>("DiaQ", true);
			window.InitEditor();
		}

		[MenuItem("Help/DiaQ Documentation", false, 99)]
		public static void ShowDiaQDocs()
		{
			Application.OpenURL("http://plyoung.com/unirpg/docs/diaq.html");
		}

		public void OnFocus()
		{
			wantsMouseMove = true;
			if (asset == null) InitEditor();
		}

		public void OnLostFocus()
		{
			wantsMouseMove = false;
			linkNode = null;
		}

		public void InitEditor()
		{
			currGraph = null;
			currNode = null;
			currQuest = null;
			LoadAsset(true);
		}

		private static void LoadAsset(bool forceReload)
		{
			if (asset != null && !forceReload) return;

			assetsLocation = EditorPrefs.GetString(GetProjectName() + "_DiaQ_assetLocation", assetsLocation);
			assetsLocation = DiaQEdUtil.CheckRelativePathExist(assetsLocation, "Resources");
			EditorPrefs.SetString(GetProjectName() + "_DiaQ_assetLocation", assetsLocation);

			// load the graphs
			asset = (DiaQAsset)Resources.LoadAssetAtPath(assetsLocation + assetFile, typeof(DiaQAsset));
			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<DiaQAsset>();
				AssetDatabase.CreateAsset(asset, assetsLocation + assetFile);
				AssetDatabase.Refresh();
			}
		}

		public void OnGUI()
		{
			if (asset == null) { InitEditor(); return; }
			DiaQEdGUI.UseSkin();

			if (splitterPos[0] == 0f)
			{
				splitterPos[0] = this.position.width - inspectorMinWidth;
				inspectorWidthPercentage = splitterPos[0] / this.position.width;
				prevScreenSize = new Vector2(this.position.width, this.position.height);
			}

			mainRect = new Rect(0, 0, splitterPos[0], this.position.height);
			splitterRect[0] = new Rect(splitterPos[0], 0f, 5f, this.position.height);
			inspectorRect = new Rect(splitterPos[0] + 5f, 0f, this.position.width - splitterPos[0] - 5f, this.position.height);

			if (questRect.x == 0f)
			{
				questRect.x = splitterPos[0] * 0.5f - questRect.width * 0.5f;
				questRect.height = this.position.height - 50;
			}

			// main graph area
			GUILayout.BeginArea(mainRect);
			if (currGraph!=null) 
			{
				DrawGraphArea();
			}
			else if(currQuest !=null) 
			{
				BeginWindows();
				GUI.Window(0, questRect, QuestInfoWindow, "Quest", DiaQEdGUI.NodeWindowStyle);
				EndWindows();
			}
			GUILayout.EndArea();

			// splitter
			GUI.Box(splitterRect[0], GUIContent.none, DiaQEdGUI.SplitterStyle);

			// inspector
			GUILayout.BeginArea(inspectorRect);
			DrawInspector();
			GUILayout.EndArea();

			// handle events
			HandleEvents();

			// done
			if (GUI.changed)
			{
				EditorUtility.SetDirty(asset);
			}
		}

		private void HandleEvents()
		{
			if (Event.current != null)
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
					{
						if (linkNode != null)
						{
							linkNode = null;
							Event.current.Use();
							Repaint();
						}

						else
						{
							if (Event.current.button == 0)
							{
								if (currGraph != null)
								{	// Start dragging Graph
									if (mainRect.Contains(Event.current.mousePosition))
									{
										draggingGraph = true;
										Event.current.Use();
									}
								}

								if (Event.current.type != EventType.Used)
								{
									if (splitterRect[0].Contains(Event.current.mousePosition))
									{	// Start dragging vertical splitter
										draggingSplitter[0] = true;
										Event.current.Use();
									}
									else if (splitterRect[1].Contains(Event.current.mousePosition))
									{	// Start dragging horizontal splitter
										draggingSplitter[1] = true;
										Event.current.Use();
									}
								}
							}

							if (Event.current.button == 1)
							{	 // unselect node on right-click

								if (mainRect.Contains(Event.current.mousePosition))
								{
									ShowGraphAreaPopMenu();
									Event.current.Use();
									Repaint();
								}
							}
						}

					} break;

					case EventType.MouseDrag:
					{
						if (draggingGraph && currGraph!=null)
						{							
							currGraph.DrawOffset += Event.current.delta;
							Event.current.Use();
						}

						if (draggingSplitter[0]) 
						{
							splitterPos[0] += Event.current.delta.x;
							if (splitterPos[0] > (this.position.width - inspectorMinWidth)) splitterPos[0] = this.position.width - inspectorMinWidth;
							if (splitterPos[0] < 50) splitterPos[0] = 50;
							inspectorWidthPercentage = splitterPos[0] / this.position.width;
							questRect.x = splitterPos[0] * 0.5f - questRect.width * 0.5f;
							Event.current.Use();
						}

						if (draggingSplitter[1])
						{
							splitterPos[1] += Event.current.delta.y;
							if (splitterPos[1] < inspectorMinHeight) splitterPos[1] = inspectorMinHeight;
							if (splitterPos[1] > this.position.height - inspectorMinHeight) splitterPos[1] = this.position.height - inspectorMinHeight;
							Event.current.Use();
						}

					} break;

					case EventType.MouseUp:
					{
						if (draggingGraph)
						{
							draggingGraph = false;
							Event.current.Use();
							Repaint();
						}

						if (draggingSplitter[0])
						{
							draggingSplitter[0] = false;
							Event.current.Use();
							Repaint();
						}

						if (draggingSplitter[1])
						{
							draggingSplitter[1] = false;
							Event.current.Use();
							Repaint();
						}

					} break;

					case EventType.KeyDown:
					{
						if (linkNode != null)
						{
							linkNode = null;
							Event.current.Use();
							Repaint();
						}

						else
						{
							if (!GUI.changed && currNode != null)
							{
								//if (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace)
								if (Event.current.keyCode == KeyCode.Delete)
								{
									DeleteCurrentNode();
								}

								if (Event.current.keyCode == KeyCode.Escape)
								{	// Unselect Node
									currNode = null;
									Event.current.Use();
									Repaint();
								}
							}
						}

					} break;
				}

				// check if window was resized
				if (this.position.width != prevScreenSize.x)
				{
					splitterPos[0] = this.position.width * inspectorWidthPercentage;
					prevScreenSize.x = this.position.width;
					if (splitterPos[0] > (this.position.width - inspectorMinWidth) || splitterPos[0] < 50f) splitterPos[0] = this.position.width - inspectorMinWidth;
					questRect.x = splitterPos[0] * 0.5f - questRect.width * 0.5f;
					questRect.height = this.position.height - 50;
				}
				if (this.position.height != prevScreenSize.y)
				{
					prevScreenSize.y = this.position.height;
					questRect.height = this.position.height - 50;
				}
			}
		}

		private static string GetProjectName()
		{
			string[] s = Application.dataPath.Split('/');
			string projectName = s[s.Length - 2];
			return projectName;
		}

		#endregion
		// ============================================================================================================
		#region pub

		public static DiaQAsset Asset
		{
			get
			{
				if (asset == null) LoadAsset(true);
				return asset;
			}
		}

		public static DiaQGraph FindGraph(string ident)
		{
			LoadAsset(false);
			if (asset == null) return null;
			if (string.IsNullOrEmpty(ident)) return null;

			return asset.graphs.FirstOrDefault(g => g.IdentString.Equals(ident));
		}

		public static DiaQuest FindQuest(string ident)
		{
			LoadAsset(false);
			if (asset == null) return null;
			if (string.IsNullOrEmpty(ident)) return null;

			return asset.quests.FirstOrDefault(q => q.IdentString.Equals(ident));
		}

		#endregion
		// ============================================================================================================
		#region graph/main area

		private void DrawGraphArea()
		{
			// draw the "mini-map" when moving graph around
			if (draggingGraph)
			{
				DrawGraphMiniMap();
			}

			// draw the node connections
			Handles.BeginGUI();
			{
				foreach (DiaQNode n in currGraph.nodes)
				{
					if (n == null) { Debug.Log("n = null"); continue; }
					if (n.outputs == null) { Debug.Log("out = null"); continue; }

					DiaQNode.NodeLink del = null;
					foreach(DiaQNode.NodeLink nl in n.outputs)
					{
						DiaQNode targetNode = currGraph.GetNode(nl.targetNodeId);
						if (DiaQEdGUI.DrawNodeCurve(mainRect, n.OutputPositionInGraph(currGraph, nl.outSlot), targetNode.InputPositionInGraph(currGraph), GetLinkColour(n, nl.outSlot)))
						{
							del = nl;
						}
					}

					if (del != null)
					{
						n.outputs.Remove(del);
						EditorUtility.SetDirty(asset);
					}
				}
			}
			Handles.EndGUI();

			// draw the node windows
			BeginWindows();
			{
				foreach (DiaQNode node in currGraph.nodes)
				{
					Rect r = node.GetRectInGraph(currGraph);

					// first check if visible. dont bother drawing if clipped.
					var topLeft = new Vector2(r.xMin, r.yMin);
					var topRight = new Vector2(r.xMax, r.yMin);
					var bottomLeft = new Vector2(r.xMin, r.yMax);
					var bottomRight = new Vector2(r.xMax, r.yMax);
					if (mainRect.Contains(topLeft) || mainRect.Contains(topRight) || mainRect.Contains(bottomLeft) || mainRect.Contains(bottomRight))
					{
						if (node == currNode) GUI.color = DiaQEdGUI.Col_SelectedNode;
						r = GUI.Window(node.id, r, NodeWindow, node.type.ToString(), DiaQEdGUI.NodeWindowStyle);
						node.SetRectInGraph(currGraph, r);
						GUI.color = Color.white;

						if (showComments && !string.IsNullOrEmpty(node.comment))
						{
							float wMin, wMax;
							GUIContent content = new GUIContent(node.comment);
							GUI.skin.box.CalcMinMaxWidth(content, out wMin, out wMax);
							var w = Mathf.Min(r.width * 1.5f, wMax);
							var h = GUI.skin.box.CalcHeight(content, w);

							var oldColor = GUI.color;
							GUI.color = DiaQEdGUI.Col_Comment;
							GUI.Box(new Rect(r.x, r.yMax, w, h), content, DiaQEdGUI.CommentBoxStyle);
							GUI.color = oldColor;
						}
					}
				}
			}
			EndWindows();

			// draw toolbar
			DrawGraphToolbar();

			// draw the bezier for making a new connection
			if (linkNode != null) DrawNewConnection();
		}

		private void DrawGraphMiniMap()
		{
			Color oldColor = GUI.color;
			Color trans = GUI.color * new Color(1f, 1f, 1f, 0.3f);

			// draw the bounds of graph
			Vector2 drawCenter = new Vector2((mainRect.x + (mainRect.width * 0.5f)), mainRect.y + (mainRect.height * 0.5f));
			drawCenter.x -= mainRect.width * 0.1f;
			drawCenter.y -= mainRect.height * 0.1f;

			Vector2 redSize = new Vector2(mainRect.width, mainRect.height) * 0.2f;

			GUI.color = trans;
			GUI.Box(new Rect(drawCenter.x, drawCenter.y, redSize.x, redSize.y), "");
			GUI.color = oldColor;

			// draw the mini graph
			foreach (DiaQNode node in currGraph.nodes)
			{
				Rect rect = node.GetRectInGraph(currGraph);
				Vector2 delta = new Vector2(rect.x, rect.y);
				Vector2 size = new Vector2(rect.width, rect.height);
				delta *= 0.2f; size *= 0.2f;
				delta += drawCenter;
				GUI.Box(new Rect(delta.x, delta.y, size.x, size.y), GUIContent.none);
			}
		}

		private void DrawGraphToolbar()
		{
			GUILayout.Space(15);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_ResetView, "Reset view"), DiaQEdGUI.ButtonLeftStyle, GUILayout.Width(30))) { currGraph.DrawOffset = Vector2.zero; linkNode = null; }
				showComments = GUILayout.Toggle(showComments, new GUIContent(DiaQEdGUI.Icon_Tag, "Toggle Comments"), DiaQEdGUI.ButtonRightStyle, GUILayout.Width(30));

				GUILayout.Space(10);
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Dialogue, "Add Dialogue Node"), DiaQEdGUI.ButtonLeftStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.Dialogue, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Quest, "Add GiveQuest Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.GiveQuest, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Check, "Add UpdateCondition Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.UpdateCondition, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
#if UNIRPG_CORE
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Reward, "Add GiveReward Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.GiveReward, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Event, "Add UniRPGEvent Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.UniRPGEvent, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
#endif
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_QuestCheck, "Add QuestCheck Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.QuestCheck, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Decision, "Add Decision Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.Decision, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Dice, "Add RandomPath Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.Random, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Variable, "Add SetVariable Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.SetVariable, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Script, "Add SendMessage Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.SendMessage, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Bug, "Add DebugLog Node"), DiaQEdGUI.ButtonMidStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.DebugLog, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Stop, "Add End Node"), DiaQEdGUI.ButtonRightStyle, GUILayout.Width(30))) { currGraph.CreateNode(DiaQNode.Type.End, new Vector2(splitterPos[0] * 0.5f, this.position.height * 0.5f)); linkNode = null; }
			} 
			GUILayout.EndHorizontal();
		}

		private void ShowGraphAreaPopMenu()
		{
			if (currGraph == null) return;

			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Add Dialogue Node"), false, OnGraphPopMenu, new object[] { 1, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add GiveQuest Node"), false, OnGraphPopMenu, new object[] { 4, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add UpdateCondition Node"), false, OnGraphPopMenu, new object[] { 10, Event.current.mousePosition });
#if UNIRPG_CORE
			menu.AddItem(new GUIContent("Add GiveReward Node"), false, OnGraphPopMenu, new object[] { 8, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add UniRPGEvent Node"), false, OnGraphPopMenu, new object[] { 11, Event.current.mousePosition });
#endif
			menu.AddItem(new GUIContent("Add QuestCheck Node"), false, OnGraphPopMenu, new object[] { 12, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add Decision Node"), false, OnGraphPopMenu, new object[] { 2, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add RandomPath Node"), false, OnGraphPopMenu, new object[] { 9, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add SetVariable Node"), false, OnGraphPopMenu, new object[] { 7, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add SendMessage Node"), false, OnGraphPopMenu, new object[] { 6, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add DebugLog Node"), false, OnGraphPopMenu, new object[] { 5, Event.current.mousePosition });
			menu.AddItem(new GUIContent("Add End Node"), false, OnGraphPopMenu, new object[] { 3, Event.current.mousePosition });
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Reset view"), false, OnGraphPopMenu, new object[] { 200 });
			if (currNode!=null) menu.AddItem(new GUIContent("Clear selected"), false, OnGraphPopMenu, new object[] { 201 });
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Delete Graph"), false, OnGraphPopMenu, new object[] { 300 });

			menu.ShowAsContext();
		}

		private void OnGraphPopMenu(object arg)
		{
			object[] args = (object[])arg;
			int opt = (int)args[0];

			switch (opt)
			{
				case 1: currGraph.CreateNode(DiaQNode.Type.Dialogue, (Vector2)args[1]); break;
				case 2: currGraph.CreateNode(DiaQNode.Type.Decision, (Vector2)args[1]); break;
				case 3: currGraph.CreateNode(DiaQNode.Type.End, (Vector2)args[1]); break;
				case 4: currGraph.CreateNode(DiaQNode.Type.GiveQuest, (Vector2)args[1]); break;
				case 5: currGraph.CreateNode(DiaQNode.Type.DebugLog, (Vector2)args[1]); break;
				case 6: currGraph.CreateNode(DiaQNode.Type.SendMessage, (Vector2)args[1]); break;
				case 7: currGraph.CreateNode(DiaQNode.Type.SetVariable, (Vector2)args[1]); break;
				case 9: currGraph.CreateNode(DiaQNode.Type.Random, (Vector2)args[1]); break;
				case 10: currGraph.CreateNode(DiaQNode.Type.UpdateCondition, (Vector2)args[1]); break;
				case 12: currGraph.CreateNode(DiaQNode.Type.QuestCheck, (Vector2)args[1]); break;

#if UNIRPG_CORE
				case 8: currGraph.CreateNode(DiaQNode.Type.GiveReward, (Vector2)args[1]); break;
				case 11: currGraph.CreateNode(DiaQNode.Type.UniRPGEvent, (Vector2)args[1]); break;
#endif

				case 200: currGraph.DrawOffset = Vector2.zero; currNode = null; break;
				case 201: currNode = null; break;

				case 300: DeleteCurrentGraph(); break;
			}
		}

		private void DrawNewConnection()
		{
			if (Event.current != null)
			{
				DiaQEdGUI.DrawNodeCurve(mainRect, linkOutPos1, Event.current.mousePosition, Color.black);
				if (Event.current.type == EventType.MouseMove) Repaint();
			}
		}

		#endregion
		// ============================================================================================================
		#region node window

		private void NodeWindow(int id)
		{
			DiaQNode node = currGraph.GetNode(id);
			
			switch (node.type)
			{
				case DiaQNode.Type.Start: DrawNode_Start(node); break;
				case DiaQNode.Type.End: DrawNode_End(node); break;
				case DiaQNode.Type.Dialogue: DrawNode_Dialogue(node); break;
				case DiaQNode.Type.Decision: DrawNode_Decision(node); break;
				case DiaQNode.Type.GiveQuest: DrawNode_Quest(node); break;
				case DiaQNode.Type.DebugLog: DrawNode_DebugLog(node); break;
				case DiaQNode.Type.SendMessage: DrawNode_SendMessage(node); break;
				case DiaQNode.Type.SetVariable: DrawNode_SetVariable(node); break;
				case DiaQNode.Type.Random: DrawNode_Random(node); break;
				case DiaQNode.Type.QuestCheck: DrawNode_QuestCheck(node); break;
				case DiaQNode.Type.UpdateCondition: DrawNode_UpdateCondition(node); break;
#if UNIRPG_CORE
				case DiaQNode.Type.GiveReward: DrawNode_Reward(node); break;
				case DiaQNode.Type.UniRPGEvent: DrawNode_UniRPGEvent(node); break;
#endif
			}

			if (node.type != DiaQNode.Type.Start)
			{
				if (GUI.Button(new Rect(0, 0, 16, 16), new GUIContent(DiaQEdGUI.Icon_ConnectionIn), DiaQEdGUI.IconButtonStyle))
				{
					if (linkNode != null)
					{
						linkNode.LinkWith(node, linkOutSlot, true);
						linkNode = null;
						EditorUtility.SetDirty(asset);
					}
				}
			}

			if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown))
			{
				currNode = node;
				GUI.FocusControl("");
				GUI.FocusWindow(id);
			}

			GUI.DragWindow();
		}

		private void DrawNode_Start(DiaQNode node)
		{
			GUI.DrawTexture(new Rect(19, 19, 16, 16), DiaQEdGUI.Icon_Play);
			Vector2 v = node.OutputPosition(0);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_End(DiaQNode node)
		{
			GUI.DrawTexture(new Rect(node.position.width * 0.5f - 8, 20, 16, 16), DiaQEdGUI.Icon_Stop);
		}

		private void DrawNode_Dialogue(DiaQNode node)
		{
			GUI.DrawTexture(new Rect(node.position.width - 14, 2, 12, 12), DiaQEdGUI.Icon_Dialogue);

			for (int i = 0; i < node.choices.Count; i++)
			{
				Vector2 v = node.OutputPosition(i);
				GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), node.choices[i] + " :" + (i + 1).ToString(), DiaQEdGUI.RightAlignTextStyle);
				if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
				{
					linkNode = node;
					linkOutSlot = i;
					linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
				}
			}
		}

		private void DrawNode_Decision(DiaQNode node)
		{
			GUI.DrawTexture(new Rect(node.position.width - 14, 2, 12, 12), DiaQEdGUI.Icon_Decision);

			Vector2 v = node.OutputPosition(0);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "TRUE", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowGreen), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}

			v = node.OutputPosition(1);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "FALSE", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowRed), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 1;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_Quest(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Quest);
			if (node.i_data[0]==0) GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.CachedString, DiaQEdGUI.LeftAlignTextStyle);
			else GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), "from: " + node.data[0], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_DebugLog(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Bug);

			if (node.i_data[0] >= 2 && node.i_data[0] <= 4) GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.CachedString, DiaQEdGUI.LeftAlignTextStyle);
			else GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.data[0], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_SendMessage(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Script);
			GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.data[1], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_SetVariable(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Variable);
			GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.data[0], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_Random(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Dice);

			for (int i = 0; i < node.i_data[0]; i++)
			{
				v = node.OutputPosition(i);
				GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), i.ToString(), DiaQEdGUI.RightAlignTextStyle);
				if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
				{
					linkNode = node;
					linkOutSlot = i;
					linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
				}
			}
		}

		private void DrawNode_UpdateCondition(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Check);
			GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.data[0], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_QuestCheck(DiaQNode node)
		{
			GUI.DrawTexture(new Rect(node.position.width - 14, 2, 12, 12), DiaQEdGUI.Icon_Decision);

			Vector2 v = node.OutputPosition(0);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "NotTaken", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowRed), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}

			v = node.OutputPosition(1);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "Accepted", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowYellow), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 1;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}

			v = node.OutputPosition(2);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "Completed", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowGreen), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 2;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}

			v = node.OutputPosition(3);
			GUI.Label(new Rect(0, v.y - 9, node.position.width - 20, 16), "HandedIn", DiaQEdGUI.RightAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 3;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}

			v = node.OutputPosition(4);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Quest);
			if (node.i_data[0] == 0) GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.CachedString, DiaQEdGUI.LeftAlignTextStyle);
			else GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), "from: " + node.data[0], DiaQEdGUI.LeftAlignTextStyle);

		}

#if UNIRPG_CORE
		private void DrawNode_Reward(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Reward);
			if (node.i_data[0] == 0) GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.CachedString, DiaQEdGUI.LeftAlignTextStyle);
			else GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), "from: " + node.data[0], DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}

		private void DrawNode_UniRPGEvent(DiaQNode node)
		{
			Vector2 v = node.OutputPosition(0);
			GUI.DrawTexture(new Rect(2, v.y - 8, 16, 16), DiaQEdGUI.Icon_Event);
			GUI.Label(new Rect(20, v.y - 9, node.position.width - 40, 16), node.CachedString, DiaQEdGUI.LeftAlignTextStyle);
			if (GUI.Button(new Rect(v.x - 16, v.y - 8, 16, 16), new GUIContent(DiaQEdGUI.Icon_ArrowBlue), DiaQEdGUI.IconButtonStyle))
			{
				linkNode = node;
				linkOutSlot = 0;
				linkOutPos1 = node.GetPositionInGraph(currGraph) + v;
			}
		}
#endif

		#endregion
		// ============================================================================================================
		#region inspector

		private void DrawInspector()
		{
			// toolbar
			GUILayout.BeginHorizontal();
			{
				if (inspectorArea != 0 && inspectorArea != 2) GUI.enabled = false;
				if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
				{
					if (inspectorArea == 0)
					{	// new dialogue graph
						currGraph = new DiaQGraph();
						currGraph.name = "Dialogue";
						currGraph.CreateNode(DiaQNode.Type.Start, new Vector2(100f, 70f));
						asset.graphs.Add(currGraph);
						EditorUtility.SetDirty(asset);
					}
					else
					{	// new quest
						currQuest = new DiaQuest();
						currQuest.name = "Quest";
						asset.quests.Add(currQuest);
						EditorUtility.SetDirty(asset);
					}
				}
				GUI.enabled = true;

				if (DiaQEdGUI.ToggleButton(inspectorArea == 0, "Graphs", EditorStyles.toolbarButton))
				{
					currQuest = null;
					inspectorArea = 0;
					inspectorScroll[0] = Vector2.zero;
				}

				if (DiaQEdGUI.ToggleButton(inspectorArea == 2, "Quests", EditorStyles.toolbarButton))
				{
					currGraph = null;
					currNode = null;
					inspectorArea = 2;
					inspectorScroll[0] = Vector2.zero;
				}

				if (DiaQEdGUI.ToggleButton(inspectorArea == 1, "Settings", EditorStyles.toolbarButton))
				{
					currGraph = null;
					currNode = null;
					currQuest = null;
					inspectorArea = 1;
					inspectorScroll[0] = Vector2.zero;
				}

				if (GUILayout.Button(new GUIContent(DiaQEdGUI.Icon_Help), EditorStyles.toolbarButton, GUILayout.Width(25)))
				{
					DiaQVersion.ShowAbout();
				}
			}
			GUILayout.EndHorizontal();
			DiaQEdGUI.DrawHorizontalLine(2f, DiaQEdGUI.Col_Back, 0f, 2f);

			if (inspectorArea == 0)
			{
				// list of graphs
				inspectorScroll[0] = GUILayout.BeginScrollView(inspectorScroll[0], false, true, GUILayout.Width(inspectorRect.width), GUILayout.Height(splitterPos[1] - 18));
				{
					foreach (DiaQGraph g in asset.graphs)
					{
						if (g == null) continue;
						if (DiaQEdGUI.ToggleButton(g == currGraph, g.name, EditorStyles.miniButton))
						{
							currNode = null;
							currGraph = g;
							UpdateGraphCachedStrings();
							GUI.FocusControl("");
						}
					}
				}
				GUILayout.EndScrollView();

				// splitter
				if (splitterPos[1] == 0f) splitterPos[1] = this.position.height * 0.5f;
				splitterRect[1] = new Rect(0f, splitterPos[1], this.position.width, 5f);
				GUI.Box(splitterRect[1], GUIContent.none, DiaQEdGUI.SplitterStyle);
				GUILayout.Space(1);

				// properties of selected node/ graph
				inspectorScroll[1] = GUILayout.BeginScrollView(inspectorScroll[1], false, true, GUILayout.Width(inspectorRect.width));
				{
					if (currNode != null) DrawNodeProperties();
					else if (currGraph != null) DrawGraphProperties();
				}
				GUILayout.EndScrollView();
				//GUILayout.Space(23);
			}

			else if (inspectorArea == 1)
			{	// show settings/ tools
				inspectorScroll[0] = GUILayout.BeginScrollView(inspectorScroll[0], false, true, GUILayout.Width(inspectorRect.width));
				{
					DrawInspectorTools();
				}
				GUILayout.EndScrollView();
				//GUILayout.Space(23);
			}

			else if (inspectorArea == 2)
			{	// show list of quests
				inspectorScroll[0] = GUILayout.BeginScrollView(inspectorScroll[0], false, true, GUILayout.Width(inspectorRect.width));
				{
					DrawQuestList();
				}
				GUILayout.EndScrollView();
				//GUILayout.Space(23);
			}
		}

		private void DrawGraphProperties()
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Graph Properties", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50)))
				{
					if (DeleteCurrentGraph())
					{
						GUIUtility.ExitGUI();
						return;
					}
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUIUtility.LookLikeControls(70);
			currGraph.name = EditorGUILayout.TextField("Name", currGraph.name);
			EditorGUILayout.TextField("Ident", currGraph.IdentString);
			currGraph.customIdent = EditorGUILayout.TextField("CustomID", currGraph.customIdent);
		}

		#endregion
		// ============================================================================================================
		#region inspector - nodes

		private void DrawNodeProperties()
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Node Properties", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50)))
				{
					if (DeleteCurrentNode())
					{
						GUIUtility.ExitGUI();
						return;
					}
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			switch (currNode.type)
			{
				case DiaQNode.Type.Dialogue: DrawDialogueNodeProps(); break;
				case DiaQNode.Type.GiveQuest: DrawQuestNodeProps(); break;
				case DiaQNode.Type.DebugLog: DrawwDebugLogProps(); break;
				case DiaQNode.Type.Decision: DiaQDecisionEd.OnGUI(this, currNode.decision, asset, inspectorRect.width); break;
				case DiaQNode.Type.SendMessage: DrawSendMessageProps(); break;
				case DiaQNode.Type.SetVariable: DrawSetVariableProps(); break;
				case DiaQNode.Type.Random: DrawRandomProps(); break;
				case DiaQNode.Type.UpdateCondition: DrawUpdateConditionProps(); break;
				case DiaQNode.Type.QuestCheck: DrawQuestCheckProps(); break;

#if UNIRPG_CORE
				case DiaQNode.Type.GiveReward: DrawRewardProps(); break;
				case DiaQNode.Type.UniRPGEvent: DrawUniRPGEventProps(); break;
#endif
			}

			EditorGUILayout.Space();
			GUILayout.Label("Comment");
			currNode.comment = EditorGUILayout.TextArea(currNode.comment, DiaQEdGUI.TextAreaStyle);
			GUILayout.Space(20);
		}

		private void DrawDialogueNodeProps()
		{
			GUILayout.Label("Conversation Text");
			currNode.data[0] = EditorGUILayout.TextArea(currNode.data[0], DiaQEdGUI.TextAreaStyle);
			EditorGUILayout.Space();

			// quest link (if any)
			GUILayout.BeginHorizontal();
			{
				currNode.toggle[0] = GUILayout.Toggle(currNode.toggle[0], " Link Quest Data", GUILayout.Width(120));
				if (currNode.toggle[0])
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			if (currNode.toggle[0]) currNode.toggle[1] = GUILayout.Toggle(currNode.toggle[1], " Append Quest Text to Conversation Text");
			EditorGUILayout.Space();

			// show choices/options
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Choices");
				if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30)))
				{
					currNode.choices.Add("");
					currNode.SetRectInGraph(currGraph, DiaQNode.GetNodeDefaultRect(currNode, currNode.GetCenterPositionInGraph(currGraph)));
					EditorUtility.SetDirty(asset);
				}
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			int del = -1;
			for (int i = 0; i < currNode.choices.Count; i++)
			{
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(25))) del = i;
					GUILayout.Label((i + 1).ToString(), GUILayout.Width(20));
					currNode.choices[i] = EditorGUILayout.TextField(currNode.choices[i], DiaQEdGUI.TextAreaStyle);
				}
				GUILayout.EndHorizontal();
			}

			if (del >= 0)
			{
				currNode.choices.RemoveAt(del);
				currNode.SetRectInGraph(currGraph, DiaQNode.GetNodeDefaultRect(currNode, currNode.GetCenterPositionInGraph(currGraph)));
				EditorUtility.SetDirty(asset);
			}

		}

		private void DrawQuestNodeProps()
		{
			GUILayout.Label("Quest to give");

			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 0, " selected", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 0;
					currNode.data[0] = "";
					currNode.CachedString = "-";
				}
				if (currNode.i_data[0] == 0)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 1, " from diaq var", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 1;
					currNode.data[0] = "";
					currNode.CachedString = "";
				}
				if (currNode.i_data[0] == 1)
				{
					currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, null);
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawwDebugLogProps()
		{
			GUILayout.Label("Write to console ...");

			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 1, " val of diaq var named", EditorStyles.toggle, GUILayout.Width(150)))
				{
					currNode.data[0] = "";
					currNode.i_data[0] = 1; 
				}
				if (currNode.i_data[0] == 1)
				{
					currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 2, " quest accepted", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.data[0] = "";
					currNode.i_data[0] = 2;
				}
				if (currNode.i_data[0] == 2)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 3, " quest completed", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.data[0] = "";
					currNode.i_data[0] = 3;
				}
				if (currNode.i_data[0] == 3)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 4, " quest handed-in", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.data[0] = "";
					currNode.i_data[0] = 4;
				}
				if (currNode.i_data[0] == 4)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();

			if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 0, " text", EditorStyles.toggle))
			{
				currNode.data[0] = "";
				currNode.i_data[0] = 0;
			}
			if (currNode.i_data[0] == 0) currNode.data[0] = EditorGUILayout.TextArea(currNode.data[0], DiaQEdGUI.TextAreaStyle);
		}

		private void DrawSendMessageProps()
		{
			EditorGUIUtility.LookLikeControls(60);
			currNode.data[1] = EditorGUILayout.TextField("Call", currNode.data[1]);
			EditorGUILayout.Space();
			EditorGUIUtility.LookLikeControls();

			GUILayout.Label("on all object(s) ...");
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(currNode.i_data[0] == 0, " with tag", GUILayout.Width(100))) currNode.i_data[0] = 0;
				if (currNode.i_data[0] == 0) currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(currNode.i_data[0] == 2, " of type", GUILayout.Width(100))) currNode.i_data[0] = 2;
				if (currNode.i_data[0] == 2) currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Toggle(currNode.i_data[0] == 1, " with name", GUILayout.Width(100))) currNode.i_data[0] = 1;
				if (currNode.i_data[0] == 1) currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			GUILayout.Label("with data ...");
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 0, " none", EditorStyles.toggle, GUILayout.Width(100))) currNode.i_data[1] = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 4, " quest ident", EditorStyles.toggle, GUILayout.Width(100)))
				{
					currNode.data[2] = "";
					currNode.CachedString = "-";
					currNode.i_data[1] = 4;
				}
				if (currNode.i_data[1] == 4)
				{
					if (GUILayout.Button(currNode.CachedString, EditorStyles.miniButton)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 5, " val of diaq var named", EditorStyles.toggle, GUILayout.Width(150)))
				{
					currNode.data[2] = "";
					currNode.i_data[1] = 5;
				}
				if (currNode.i_data[1] == 5)
				{
					currNode.data[2] = EditorGUILayout.TextField(currNode.data[2]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 1, " string", EditorStyles.toggle, GUILayout.Width(100)))
				{
					currNode.data[2] = "";
					currNode.i_data[1] = 1;
				}
				if (currNode.i_data[1] == 1) currNode.data[2] = EditorGUILayout.TextField(currNode.data[2]);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 2, " number", EditorStyles.toggle, GUILayout.Width(100))) currNode.i_data[1] = 2;
				if (currNode.i_data[1] == 2) currNode.f_data[0] = EditorGUILayout.FloatField(currNode.f_data[0]);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[1] == 3, " prefab", EditorStyles.toggle, GUILayout.Width(100))) currNode.i_data[1] = 3;
				if (currNode.i_data[1] == 3) currNode.o_data[0] = EditorGUILayout.ObjectField(currNode.o_data[0], typeof(Object), false);
			}
			GUILayout.EndHorizontal();

			if (currNode.i_data[1] != 3) currNode.o_data[0] = null;
		}

		private void DrawSetVariableProps()
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Set Diaq Var: ");
				currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
				if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, new object[] { 0 });
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			GUILayout.Label("to ...");

			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 0, " value", EditorStyles.toggle, GUILayout.Width(100)))
				{
					currNode.data[1] = "";
					currNode.i_data[0] = 0;
				}
				if (currNode.i_data[0] == 0) currNode.data[1] = EditorGUILayout.TextField(currNode.data[1]);
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 1, " val of diaq var named", EditorStyles.toggle, GUILayout.Width(150)))
				{
					currNode.data[1] = "";
					currNode.i_data[0] = 1;
				}
				if (currNode.i_data[0] == 1)
				{
					currNode.data[1] = EditorGUILayout.TextField(currNode.data[1]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, new object[] { 1 });
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 2, " quest ident", EditorStyles.toggle, GUILayout.Width(100)))
				{
					currNode.data[1] = "";
					currNode.i_data[0] = 2;
					currNode.CachedString = "-";
				}
				if (currNode.i_data[0] == 2)
				{
					if (GUILayout.Button(currNode.CachedString, EditorStyles.miniButton)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();


		}

		private void DrawRandomProps()
		{
			EditorGUI.BeginChangeCheck();
			currNode.i_data[0] = EditorGUILayout.IntField("Number of paths", currNode.i_data[0]);
			if (EditorGUI.EndChangeCheck())
			{
				if (currNode.i_data[0] < 1) currNode.i_data[0] = 1;
			}
		}

		private void DrawUpdateConditionProps()
		{
			currNode.data[0] = EditorGUILayout.TextField("Condition named", currNode.data[0]);
			currNode.i_data[0] = EditorGUILayout.IntField("Value", currNode.i_data[0]);
			EditorGUILayout.Space();
			GUILayout.Label("Only for specific Quest?");
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(currNode.CachedString, DiaQEdGUI.ButtonLeftStyle))
				{
					DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
				if (GUILayout.Button("x", DiaQEdGUI.ButtonRightStyle, GUILayout.Width(25)))
				{
					currNode.data[1] = "";
					currNode.CachedString = "-";
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawQuestCheckProps()
		{
			GUILayout.Label("Quest to check");

			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 0, " selected", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 0;
					currNode.data[0] = "";
					currNode.CachedString = "-";
				}
				if (currNode.i_data[0] == 0)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 1, " from diaq var", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 1;
					currNode.data[0] = "";
					currNode.CachedString = "";
				}
				if (currNode.i_data[0] == 1)
				{
					currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, null);
				}
			}
			GUILayout.EndHorizontal();
		}

#if UNIRPG_CORE
		private void DrawRewardProps()
		{
			GUILayout.Label("Give Rewards from Quest");

			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 0, " selected", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 0;
					currNode.data[0] = "";
					currNode.CachedString = "-";
				}
				if (currNode.i_data[0] == 0)
				{
					if (GUILayout.Button(currNode.CachedString)) DiaQuestSelectWiz.ShowWiz(_OnNodePropsQuestSelected, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			{
				if (DiaQEdGUI.ToggleButton(currNode.i_data[0] == 1, " from diaq var", EditorStyles.toggle, GUILayout.Width(130)))
				{
					currNode.i_data[0] = 1;
					currNode.data[0] = "";
					currNode.CachedString = "";
				}
				if (currNode.i_data[0] == 1)
				{
					currNode.data[0] = EditorGUILayout.TextField(currNode.data[0]);
					if (GUILayout.Button("<>", EditorStyles.miniButton, GUILayout.Width(30))) DiaQVarSelectWiz.ShowWiz(_OnNodePropsVarSelected, null, asset, null);
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (GUILayout.Toggle(currNode.i_data[1] == 1, " quest must be Accepted")) currNode.i_data[1] = 1; else currNode.i_data[1] = 0;
			if (GUILayout.Toggle(currNode.i_data[2] == 1, " quest must be Completed")) currNode.i_data[2] = 1; else currNode.i_data[2] = 0;
			if (GUILayout.Toggle(currNode.i_data[3] == 1, " quest must not be Handed In previously")) currNode.i_data[3] = 1; else currNode.i_data[3] = 0;
		}

		private void DrawUniRPGEventProps()
		{
			GUILayout.Label("Event to call");
			if (GUILayout.Button(currNode.CachedString))
			{
				EventSelectWiz.Show(_OnNodeEventSelected, null, 0);
			}
		}

		private void _OnNodeEventSelected(System.Object sender)
		{
			EventSelectWiz wiz = sender as EventSelectWiz;
			if (currNode != null)
			{
				if (currNode.type == DiaQNode.Type.UniRPGEvent)
				{
					currNode.data[0] = wiz.selectedEvent.id.ToString();
					currNode.CachedString = wiz.selectedEvent.screenName;
					EditorUtility.SetDirty(asset);
				}
			}
			wiz.Close();
			Repaint();
		}
#endif

		private void _OnNodePropsQuestSelected(DiaQuestSelectWiz wiz, object[] args)
		{
			if (currNode != null)
			{
				switch (currNode.type)
				{
					case DiaQNode.Type.GiveQuest:
					{
						currNode.data[0] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

					case DiaQNode.Type.Dialogue:
					{
						currNode.data[1] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

					case DiaQNode.Type.SendMessage:
					{
						currNode.data[2] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

					case DiaQNode.Type.DebugLog:
					{
						currNode.data[0] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);					
					} break;

					case DiaQNode.Type.SetVariable:
					{
						currNode.data[1] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

					case DiaQNode.Type.UpdateCondition:
					{
						currNode.data[1] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

					case DiaQNode.Type.QuestCheck:
					{
						currNode.data[0] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;

#if UNIRPG_CORE
					case DiaQNode.Type.GiveReward:
					{
						currNode.data[0] = wiz.selected.IdentString;
						currNode.CachedString = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					} break;
#endif
				}
			}
			wiz.Close();
			Repaint();
		}

		private void _OnNodePropsVarSelected(DiaQVarSelectWiz wiz, object[] args)
		{
			if (currNode != null)
			{
				if (currNode.type == DiaQNode.Type.SendMessage)
				{
					if (currNode.i_data[1] == 5)
					{
						currNode.data[2] = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					}
				}

				else if (currNode.type == DiaQNode.Type.DebugLog)
				{
					if (currNode.i_data[0] == 1)
					{
						currNode.data[0] = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					}
				}

				else if (currNode.type == DiaQNode.Type.SetVariable)
				{
					int i = (int)args[0];
					currNode.data[i] = wiz.selected.name;
					EditorUtility.SetDirty(asset);
				}

				else if (currNode.type == DiaQNode.Type.GiveQuest)
				{
					if (currNode.i_data[0] == 1)
					{
						currNode.data[0] = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					}
				}

				else if (currNode.type == DiaQNode.Type.QuestCheck)
				{
					if (currNode.i_data[0] == 1)
					{
						currNode.data[0] = wiz.selected.name;
						EditorUtility.SetDirty(asset);
					}
				}
			}
			wiz.Close();
			Repaint();
		}

		#endregion
		// ============================================================================================================
		#region inspector - settings

		private void DrawInspectorTools()
		{
			EditorGUILayout.Space();

			GUILayout.Label("Save assets in");
			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				EditorGUILayout.TextField(assetsLocation);
				GUI.enabled = true;

				if (GUILayout.Button("...", GUILayout.Width(35)))
				{
					string s = EditorUtility.SaveFolderPanel("Save assets in", assetsLocation, "");
					if (s != null)
					{
						s = DiaQEdUtil.ProjectRelativePath(s);
						if (string.IsNullOrEmpty(s))
						{
							EditorUtility.DisplayDialog("Invalid path", "You must specify a path within the project's Assets folder.", "Ok");
						}
						else
						{
							assetsLocation = DiaQEdUtil.CheckRelativePathExist(s, "Resources");
							EditorPrefs.SetString(GetProjectName() + "_DiaQ_assetLocation", assetsLocation);

							// force lloading of asset at this lcoation, if any, else make new one
							currGraph = null;
							currNode = null; 
							currQuest = null;
							LoadAsset(true);
						}
					}
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

#if UNIRPG_CORE

			asset.questListIncludeOld = GUILayout.Toggle(asset.questListIncludeOld,  " QuestList Provider includes handed-in");
			GUILayout.Space(20);
#endif

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("DiaQ Variables");
				if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30)))
				{
					asset.vars.Add(new DiaQVar() { name = "var" + (asset.vars.Count + 1).ToString() });
					EditorUtility.SetDirty(asset);
				}
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			int del = -1;
			for (int i = 0; i < asset.vars.Count; i++ )
			{
				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(30))) del = i;
					asset.vars[i].name = EditorGUILayout.TextField(asset.vars[i].name);
					asset.vars[i].val = EditorGUILayout.TextField(asset.vars[i].val);
				}
				GUILayout.EndHorizontal();
			}
			if (del >= 0)
			{
				asset.vars.RemoveAt(del);
				EditorUtility.SetDirty(asset);
			}
		}

		#endregion
		// ============================================================================================================
		#region quest list and quest window/info/edit

		private void DrawQuestList()
		{
			DiaQuest del = null;
			foreach (DiaQuest q in asset.quests)
			{
				if (q == null) continue;

				EditorGUILayout.BeginHorizontal();
				{
					if (DiaQEdGUI.ToggleButton(q == currQuest, q.name, EditorStyles.miniButtonLeft))
					{
						currQuest = q;
						GUI.FocusControl("");
#if UNIRPG_CORE
						UpdateQuestCachedStrings();
#endif
					}

					if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(30)))
					{
						del = q;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (del!=null) DeleteQuest(del);
		}

		private void QuestInfoWindow(int id)
		{
			inspectorScroll[1] = GUILayout.BeginScrollView(inspectorScroll[1], false, true, GUILayout.Width(questRect.width - 8), GUILayout.MaxWidth(questRect.width - 8), GUILayout.ExpandWidth(false));
			{
				EditorGUIUtility.LookLikeControls(90);
				currQuest.name = EditorGUILayout.TextField("Quest Name", currQuest.name);
				EditorGUILayout.TextField("Ident", currQuest.IdentString);
				currQuest.customIdent = EditorGUILayout.TextField("CustomID", currQuest.customIdent);
				EditorGUILayout.Space();

				// -- Text
				GUILayout.Label("Text/ Description");
				currQuest.text = EditorGUILayout.TextArea(currQuest.text, DiaQEdGUI.TextAreaStyle, GUILayout.Width(questRect.width - 35));
				EditorGUILayout.Space();

				// -- Conditions
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Conditions for Completion");
					if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(25)))
					{
						currQuest.conditions.Add(new DiaQuest.Condition());
						EditorUtility.SetDirty(asset);
					}
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();

				DiaQuest.Condition del = null;
				foreach (DiaQuest.Condition c in currQuest.conditions)
				{
					GUILayout.BeginVertical(GUI.skin.box);
					{
						GUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(25))) del = c;
							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(75, 100);
							c.ident = EditorGUILayout.TextField("Ident/ Key:", c.ident);
							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(50, 50);
							c.target = EditorGUILayout.IntField("Target:", c.target);
							if (c.target < 1) c.target = 1;
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.Label("Text/ Description");
						c.text = EditorGUILayout.TextArea(c.text, DiaQEdGUI.TextAreaStyle);

					}
					GUILayout.EndVertical();
				}

				if (del != null)
				{
					currQuest.conditions.Remove(del);
					EditorUtility.SetDirty(asset);
				}
				EditorGUILayout.Space();

				// -- Rewards
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Rewards");
					if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(25)))
					{
						currQuest.rewards.Add(new DiaQuest.Reward());
						EditorUtility.SetDirty(asset);
					}
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal(); EditorGUILayout.Space();

				DiaQuest.Reward delR = null;
				foreach (DiaQuest.Reward c in currQuest.rewards)
				{
					GUILayout.BeginVertical(GUI.skin.box);
					{
#if UNIRPG_CORE
						GUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(25))) delR = c;
							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(40, 120);
							EditorGUI.BeginChangeCheck();
							c.type = (DiaQuest.Reward.Type)EditorGUILayout.EnumPopup("Type:", c.type);
							if (EditorGUI.EndChangeCheck()) { c.ident = ""; c.CachedName = "-"; }
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space(80);
							switch (c.type)
							{
								case DiaQuest.Reward.Type.Currency:
								{
									GUILayout.Label(UniRPGEditorGlobal.DB.currency, GUILayout.Width(125));
								} break;
								case DiaQuest.Reward.Type.Attribute:
								{
									if (GUILayout.Button(c.CachedName, GUILayout.Width(125))) AttributeSelectWiz.Show(_OnAttributeSelected, new object[] { c });
								} break;
								case DiaQuest.Reward.Type.Item:
								{
									if (GUILayout.Button(c.CachedName, GUILayout.Width(125))) ItemSelectWiz.Show(false, _OnItemSelected, new object[] { c });
								} break;
							}

							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(50, 50);
							c.value = EditorGUILayout.IntField("Value:", c.value);
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
					}
#else
						GUILayout.BeginHorizontal();
						{
							if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(25))) delR = c;
							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(40, 120);
							c.type = (DiaQuest.Reward.Type)EditorGUILayout.EnumPopup("Type:", c.type);
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						{
							EditorGUIUtility.LookLikeControls(80, 120);
							c.ident = EditorGUILayout.TextField(" Ident/ Key:", c.ident);
							EditorGUILayout.Space();
							EditorGUIUtility.LookLikeControls(50, 50);
							c.value = EditorGUILayout.IntField("Value:", c.value);
							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
					}
#endif
					GUILayout.EndVertical();
				}

				if (delR != null)
				{
					currQuest.rewards.Remove(delR);
					EditorUtility.SetDirty(asset);
				}
			}
			GUILayout.EndScrollView();
		}

#if UNIRPG_CORE

		private void _OnAttributeSelected(object sender, object[] args)
		{
			AttributeSelectWiz wiz = sender as AttributeSelectWiz;
			DiaQuest.Reward r = args[0] as DiaQuest.Reward;
			r.ident = wiz.selectedAttrib.id.ToString();
			r.CachedName = wiz.selectedAttrib.screenName;
			wiz.Close();
			Repaint();
		}

		private void _OnItemSelected(object sender, object[] args)
		{
			ItemSelectWiz wiz = sender as ItemSelectWiz;
			DiaQuest.Reward r = args[0] as DiaQuest.Reward;
			r.ident = wiz.selectedItems[0].prefabId.ToString();
			r.CachedName = wiz.selectedItems[0].screenName;
			wiz.Close();
			Repaint();
		}

#endif

		#endregion
		// ============================================================================================================
		#region misc

		private bool DeleteCurrentGraph()
		{
			if (currGraph == null) return false;

			if (EditorUtility.DisplayDialog("Delete the Graph?", "This action can't be undone.", "Yes", "Cancel"))
			{
				asset.graphs.Remove(currGraph);
				currNode = null;
				currGraph = null;
				EditorUtility.SetDirty(asset);
				return true;
			}
			return false;
		}

		private bool DeleteCurrentNode()
		{
			if (currNode == null) return false;

			// Delete Node (can't delete node=0 which is the Start node)
			if (currNode.id > 0)
			{
				currGraph.RemoveLinksWith(currNode.id);
				currGraph.nodes.Remove(currNode);
				currNode = null;
				EditorUtility.SetDirty(asset);
				Event.current.Use();
				Repaint();
				return true;
			}
			return false;
		}

		private bool DeleteQuest(DiaQuest q)
		{
			if (q == null) return false;

			if (EditorUtility.DisplayDialog("Delete the Quest?", "This action can't be undone.", "Yes", "Cancel"))
			{
				asset.quests.Remove(q);
				if (q == currQuest) currQuest = null;
				EditorUtility.SetDirty(asset);
				return true;
			}
			return false;
		}

		private Color GetLinkColour(DiaQNode n, int slot)
		{
			if (n.type == DiaQNode.Type.Decision)
			{
				if (slot == 0) return DiaQEdGUI.Col_LinkGreen;
				else return DiaQEdGUI.Col_LinkRed;
			}
			else if (n.type == DiaQNode.Type.QuestCheck)
			{
				if (slot == 0) return DiaQEdGUI.Col_LinkRed;
				else if (slot == 1) return DiaQEdGUI.Col_LinkYellow;
				else if (slot == 2) return DiaQEdGUI.Col_LinkGreen;
				else if (slot == 3) return DiaQEdGUI.Col_LinkBlue;
			}
			return DiaQEdGUI.Col_LinkBlue;
		}

		private void UpdateGraphCachedStrings()
		{
			if (currGraph == null) return;
			foreach (DiaQNode n in currGraph.nodes)
			{
				switch (n.type)
				{
					case DiaQNode.Type.GiveQuest:
					case DiaQNode.Type.QuestCheck:
					{
						if (n.i_data[0] == 0)
						{
							if (!string.IsNullOrEmpty(n.data[0]))
							{
								DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[0]));
								if (quest != null) n.CachedString = quest.name;
								else { n.CachedString = "-"; n.data[0] = ""; }
							}
							else n.CachedString = "-";
						}
					} break;

					case DiaQNode.Type.Dialogue:
					{
						if (!string.IsNullOrEmpty(n.data[1]))
						{
							DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[1]));
							if (quest != null) n.CachedString = quest.name;
							else { n.CachedString = "-"; n.data[1] = ""; }
						} else n.CachedString = "-";
					} break;

					case DiaQNode.Type.SendMessage:
					{
						if (n.i_data[1] == 4) // quest data is send
						{
							if (!string.IsNullOrEmpty(n.data[2]))
							{
								DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[2]));
								if (quest != null) n.CachedString = quest.name;
								else { n.CachedString = "-"; n.data[2] = ""; }
							}
							else n.CachedString = "-";
						}
					} break;

					case DiaQNode.Type.DebugLog:
					{
						if (n.i_data[0] >= 2 && n.i_data[0] <= 4)
						{
							if (!string.IsNullOrEmpty(n.data[0]))
							{
								DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[0]));
								if (quest != null) n.CachedString = quest.name;
								else { n.CachedString = "-"; n.data[0] = ""; }
							}
							else n.CachedString = "-";
						}
					} break;

					case DiaQNode.Type.SetVariable:
					{
						if (n.i_data[0] == 2)
						{
							if (!string.IsNullOrEmpty(n.data[1]))
							{
								DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[1]));
								if (quest != null) n.CachedString = quest.name;
								else { n.CachedString = "-"; n.data[1] = ""; }
							}
							else n.CachedString = "-";
						}
					} break;

					case DiaQNode.Type.UpdateCondition:
					{
						if (!string.IsNullOrEmpty(n.data[1]))
						{
							DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[1]));
							if (quest != null) n.CachedString = quest.name;
							else { n.CachedString = "-"; n.data[1] = ""; }
						}
						else n.CachedString = "-";
					} break;

#if UNIRPG_CORE
					case DiaQNode.Type.GiveReward:
					{
						if (n.i_data[0] == 0)
						{
							if (!string.IsNullOrEmpty(n.data[0]))
							{
								DiaQuest quest = asset.quests.FirstOrDefault(q => q.IdentString.Equals(n.data[0]));
								if (quest != null) n.CachedString = quest.name;
								else { n.CachedString = "-"; n.data[0] = ""; }
							}
							else n.CachedString = "-";
						}
					} break;

					case DiaQNode.Type.UniRPGEvent:
					{
						if (!string.IsNullOrEmpty(n.data[0]))
						{
							RPGEvent ev = UniRPGEditorGlobal.DB.GetEvent(new GUID(n.data[0]));
							if (ev != null) n.CachedString = ev.screenName;
							else { n.CachedString = "-"; n.data[0] = ""; }
						}
						else n.CachedString = "-";
					} break;
#endif

				}
			}
		}

#if UNIRPG_CORE
		private void UpdateQuestCachedStrings()
		{
			if (currQuest == null) return;
			foreach (DiaQuest.Reward r in currQuest.rewards)
			{
				if (r.type == DiaQuest.Reward.Type.Attribute)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						RPGAttribute att = UniRPGEditorGlobal.DB.GetAttribute(new GUID(r.ident));
						if (att != null) r.CachedName = att.screenName;
					}
				}

				else if (r.type == DiaQuest.Reward.Type.Item)
				{
					if (!string.IsNullOrEmpty(r.ident))
					{
						RPGItem att = UniRPGEditorGlobal.DB.GetItem(new GUID(r.ident));
						if (att != null) r.CachedName = att.screenName;
					}
				}
			}
		}
#endif

		#endregion
		// ============================================================================================================
	}
}