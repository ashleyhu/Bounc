using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupsController : MonoBehaviour {

	[SerializeField] Transform[] pickups = new Transform[16];
	[SerializeField] Transform camera;

	int minSpeed = 1;
	int maxSpeed = 10;
	List<Transform> objs = new List<Transform>();

	// Use this for initialization
	void Start () {

		for (int i=0; i< 25; i++) {
			int pickupIndex = Random.Range(0, pickups.Length);
			Vector3 position = PositionObj();
			objs.Add((Transform) Instantiate(pickups[pickupIndex], position, new Quaternion(), this.transform));
		}
	}

	Vector3 PositionObj () {
		float xPos = Random.Range(camera.position.x - 20, camera.position.x + 20);
		float yPos = Random.Range(camera.position.y - 10, camera.position.y + 10);
		if (Mathf.Abs(xPos - 0) < 2f && Mathf.Abs(yPos - 0) < 2f) { // hardcoding the zeros since I know starting point of the player  
			return PositionObj();
		}
		return new Vector3(xPos, yPos);
	}

	void RepostitionObj(Transform obj) {
		var offset = camera.position - obj.position;
		Vector3 objPos = obj.position;

		if (offset.y > 10) {
			objPos.y = camera.position.y + 9;
			objPos.x = Random.Range(camera.position.x - 20, camera.position.x + 20);
		} 

		if (offset.y < -10) {
			objPos.y = camera.position.y - 9;
			objPos.x = Random.Range(camera.position.x - 20, camera.position.x + 20);
		}

		if (offset.x > 20) {
			objPos.x = camera.position.x + 19;
			objPos.y = Random.Range(camera.position.y - 10, camera.position.y + 10);
		}

		if (offset.x < -20) {
			objPos.x = camera.position.x - 19;
			objPos.y = Random.Range(camera.position.y - 10, camera.position.y + 10);
		}

		obj.position = objPos;
	}

	bool InRange (Transform obj) {
		return  obj.position.x >= (camera.position.x - 20) && 
				obj.position.x < (camera.position.x + 10) &&
				obj.position.y >= (camera.position.y - 10) && 
				obj.position.y < (camera.position.y + 10);
	}

	public void RemoveObj(Transform obj) {
		objs.Remove(obj);
	}

	void Update() {
		foreach (var obj in objs) {
			if (!InRange(obj)) {
				RepostitionObj(obj);
			}
		}
	}

	public void Stop() {
		foreach (var obj in objs) {
			obj.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
		}
	}
}
