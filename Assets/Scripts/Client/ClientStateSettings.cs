using UnityEngine;

// Typically we attach the settings the parent Ui object of the
// state, because virtually every single state needs some kind
// of ui. This keeps the settings close to the place where we
// are connecting components in the editor (which are often ui elements)
//
// We also therefore put a very small amount of behaviour on this
// otherwise POD class hierarchy: showing and hiding the root of the UI
[RequireComponent(typeof(RectTransform))]
public abstract class ClientStateSettings : MonoBehaviour
{
	public void Show()
	{
		gameObject.SetActive(true);
		ShowInternal();
	}
	
	public void Hide()
	{
		gameObject.SetActive(false);
		HideInternal();
	}

	protected virtual void ShowInternal() {}
	protected virtual void HideInternal() {}
}