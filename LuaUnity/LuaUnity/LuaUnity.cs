/*
  This class is intended to implement MonoBehaviour using a Lua script.
  Init() is called from the stub (see NewBehaviourScript.cs) in the Start()
  method. It finds the Lua script in the resources folder, and loads it rather
  like require() does.

  It should be fairly straightforward to implement other needed methods here.
*/

using System;
using UnityEngine;
using LuaInterface;
using System.Collections;

namespace LuaUnity
{
	public class LuaMonoBehaviour: MonoBehaviour {

		public static Lua L = null;
		public static string resourcePath = null;
		
		// This class may represent many different Lua behaviours, each with their
		// own method set.
		LuaFunction update, start,reset,
            onMouseEnter,onMouseOver,onMouseDown,onMouseUp,onMouseDrag,
            onGUI,onDestroy;

        LuaTable self;
		bool initialized = false;
		public static Hashtable objects;

		
		void AssignMethods (LuaTable env)
		{
			if (! initialized) {
				initialized = true;
				//Debug.Log ("assigning");
				update = (LuaFunction)env["Update"];
				start = (LuaFunction)env["Start"];
			    reset = (LuaFunction)env["Reset"];
			    onMouseEnter = (LuaFunction)env["OnMouseEnter"];
			    onMouseOver = (LuaFunction)env["OnMouseOver"];
			    onMouseDown = (LuaFunction)env["OnMouseDown"];
			    onMouseUp = (LuaFunction)env["OnMouseUp"];
			    onMouseDrag = (LuaFunction)env["OnMouseDrag"];
			    onGUI = (LuaFunction)env["OnGUI"];
			    onDestroy = (LuaFunction)env["OnDestroy"];
			}
		}
		
		[LuaGlobal]
		public void Reload() {
			initialized = false;
			string script = resourcePath+ScriptName+".lua";
			// will throw an exception on error...
			object[] res = L.DoFile (script);
			LuaTable env = (LuaTable)res[0];
            // Poor man's OOP! These are all called on self
			//Debug.Log ("here");
			if (env["Update"] != null || env["Start"] != null)
	    		AssignMethods (env);
			

            // the script may have an Init method,
            // which expects the object as an argument
            // and returns a table, otherwise we make
            // a new table with a 'this' field
            LuaFunction init = (LuaFunction)env["Init"];
            if (init != null) {
                self = (LuaTable)(init.Call(this)[0]);
            } else {
				L.NewTable(ScriptName);
                self = (LuaTable)L[ScriptName];
                self["this"] = this;
            }
			if (! initialized)
				AssignMethods(self);
			//Debug.Log ("here " + self.ToString ());
			objects[gameObject] = self;			
			
		}
		
		[LuaGlobal]
		public LuaMonoBehaviour Script(GameObject obj) {
			return obj.GetComponent<LuaMonoBehaviour>();	
		}
		
		string ScriptName;
		

        // called from Awake()...
		public void Init (string ScriptName) {
			this.ScriptName = ScriptName;
			if (L == null) { // we have one Lua state globally...
				resourcePath = Application.dataPath+"/Resources/";
				L = new Lua();
				L["package.path"] = resourcePath+"lua/?.lua";
				objects = new Hashtable();
				L["objects"] = objects;
			}
			LuaRegistrationHelper.TaggedInstanceMethods(L,this);
			Reload ();

		}

        // selected overrideable functions
        // http://unity3d.com/support/documentation/ScriptReference/MonoBehaviour.html
        void Start () {
            if (start != null)
                start.Call(self);
        }

		// Update is called once per frame
		void Update () {
			if (update != null)
				update.Call(self);
		}

        void Reset () {
            if (reset != null)
                reset.Call(self);
        }

        void OnMouseEnter() {
            if (onMouseEnter != null)
                onMouseEnter.Call(self);
        }
        void OnMouseOver() {
            if (onMouseOver != null)
                onMouseOver.Call(self);
        }

        void OnMouseDown() {
            if (onMouseDown != null)
                onMouseDown.Call(self);
        }

        void OnMouseUp() {
            if (onMouseUp != null)
                onMouseUp.Call(self);
        }

        void OnMouseDrag() {
            if (onMouseDrag != null)
                onMouseDrag.Call(self);
        }

        void OnGUI() {
            if (onGUI != null)
                onGUI.Call(self);
        }

        void OnDestroy() {
            if (onDestroy != null)
                onDestroy.Call(self);
        }

	}

}

