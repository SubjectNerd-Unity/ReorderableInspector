# Reorderable Inspector

Automatically turn arrays/lists into ReorderableLists in Unity inspectors. Inspired by [Alejandro Santiago's implementation](https://medium.com/developers-writing/how-about-having-nice-arrays-and-lists-in-unity3d-by-default-e4fba13d1b50).

![Sortable Array](./Docs/sortable-array.png)

# Usage

Place the folder in your project. By default, the inspector will only draw arrays marked with the `Reorderable` attribute

```C#
public class ListReorderTest : MonoBehaviour
{  
	[Reorderable]
	public string[] stringArray; // This will be drawn with a ReorderableList

	public List<string> stringList; // This will be drawn as a default array
}
```

If you want to apply the reorderable list to all arrays, edit `Editor\ReorderableArrayInspector.cs` and change `LIST_ALL_ARRAYS` to `true`

### Custom Element Names

You can set the name used for each element in the list by specifying it in the attribute. You can also set if the numbering will start from one or zero.

```C#
public class ListReorderTest : MonoBehaviour
{  
	[Reorderable("String")]
	public string[] stringArray; // Array elements listed as: "String 0"

	[Reorderable("Other String", isZeroIndex:false)]
	public List<string> stringList; // Array elements listed as: "Other String 1"
}
```

![Custom Element Names](./Docs/element-names.png)

## Additional Features

Handles arrays/lists nested inside other classes

```C#
public class ListReorderTest : MonoBehaviour
{
	[Serializable]
	public class InternalClass
	{
		public bool testBool;
		[Reorderable] public List<int> innerList;
	}

	[Header("A single class instance, with an inner sortable list")]
	public InternalClass instance;

	[Reorderable]
	[Header("A list of serialized class instances")]
	public List<InternalClass> classList;
}
```

Drag and drop objects into arrays like the default inspector

![Drag and drop](./Docs/sortable-drag-drop.jpg)

Automatic `ContextMenu` buttons.

```C#
public class ContextMenuTest : MonoBehaviour
{
	public bool isTestEnabled;

	[ContextMenu("Test Function")]
	private void MyTestFunction()
	{
		Debug.Log("Test function fired");
	}

	[ContextMenu("Test Function", isValidateFunction:true)]
	private bool TestFunctionValidate()
	{
		return isTestEnabled;
	}

	[ContextMenu("Other Test")]
	private void NonValidatedTest()
	{
		Debug.Log("Non validated test fired");
	}
}
```

![Context Menu](./Docs/context-menu.png)

## Limitations

- Only supports Unity 5 and above
- ReorderableLists of class instances may be a little rough, especially below Unity version 5.3
- Custom inspectors will not automatically gain the ability to turn arrays into reorderable lists. See next section for creating custom inspectors that allow for this functionality.

# Custom inspectors

Custom inspectors will not automatically draw arrays as ReorderableLists unless they inherit from `ReorderableArrayInspector`.

The class contains helper functions that can handle default property drawing. Below is a template for a custom inspector.

```C#
[CustomEditor(typeof(YourCustomClass))]
public class CustomSortableInspector : ReorderableArrayInspector
{
	// Called by OnEnable
	protected override void InitInspector()
	{
		base.InitInspector();
		
		// Always call DrawInspector function
		alwaysDrawInspector = true;
		
		// Do other initializations here
	}
	
	// Override this function to draw
	protected override void DrawInspector()
	{
		// Call the relevant default drawer functions here
		// The following functions will automatically draw properties
		// with ReorderableList when applicable
		/*
		// Draw all properties
		DrawDefaultSortable();

		// Like DrawPropertiesExcluding
		DrawSortableExcept("sprites");

		// Draw all properties, starting from specified property
		DrawPropertiesFrom("propertyName");

		// Draw all properties until before the specified property
		DrawPropertiesUpTo("endPropertyName");

		// Draw properties starting from startProperty, ends before endProperty
		DrawPropertiesFromUpTo("startProperty", "endProperty");
		*/
		
		// Write your custom inspector functions here
		EditorGUILayout.HelpBox("This is a custom inspector", MessageType.Info);
	}
}
```

You can also get a reference to the `ReorderableList` drawing the properties marked with `Reorderable`, allowing for further extension of the list.

The drag and drop handler for a list can also be set for handling dragging and dropping into lists of custom classes.

```C#
// SerializableObject
public class YourCustomClass : SerializableObject
{
	[Reorderable]
	public List<GameObject> enemyPrefabs;
	
	[Serializable]
	public struct AudioData
	{
		public AudioClip clip;
		public float volume;
	}
	
	[Reorderable]
	public List<AudioData> audioList;
}
```

```C#
// Custom inspector
[CustomEditor(typeof(YourCustomClass))]
public class CustomSortableInspector : ReorderableArrayInspector
{
	// Called by OnEnable
	protected override void InitInspector()
	{
		base.InitInspector();
		
		// Get enemy prefabs list property
		SerializedProperty propList = serializedObject.FindProperty("enemyPrefabs");
		
		// Modify the callbacks of the ReorderableList here. Refer here for details
		// http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
		//
		// Ideas:
		// Add a custom add dropdown, change how the elements are drawn, handle removing or adding objects
		ReorderableList listEnemies = GetSortableList(propList);
		
		// Get audio list property
		SerializedProperty propAudio = serializedObject.FindProperty("audioList");
		
		// Set the drag and drop handling function for the audio list
		SetDragDropHandler(propAudio, HandleAudioDragDrop);
	}
	
	protected void HandleAudioDragDrop(SerializedProperty propList, UnityEngine.Object[] objects)
	{
		bool didAdd = false;
		// Process the list of objects being drag and dropped into the audio list
		foreach (Object obj in objects)
		{
			AudioClip clip = obj as AudioClip;
			if (clip == null)
				continue;
			didAdd = true;
			
			// When list is expanded, current array size is the last index
			int newIdx = property.arraySize;
			// Expand list size
			property.arraySize++;
			
			// Get the last array element
			var propData = property.GetArrayElementAtIndex(newIdx);
			// And set the data
			propData.FindPropertyRelative("clip").objectReferenceValue = clip;
			propData.FindPropertyRelative("volume").floatValue = 1f;
		}
		
		// Make sure to apply modified properties
		if (didAdd)
			property.serializedObject.ApplyModifiedProperties();
	}
}
```
