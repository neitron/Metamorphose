using UnityEngine;



public class Rotator : MonoBehaviour 
{


	#region Private Unity properties

	[SerializeField] private Vector3 _eulerAngles;
	
	#endregion
	


	private void Update () 
	{
		transform.Rotate(_eulerAngles);
	}



}
