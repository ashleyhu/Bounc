using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	public enum PositionRelativeToBackground {
		UpLeft,
		UpRight,
		DownLeft,
		DownRight
	}

	[SerializeField] float speed = 0;
	[SerializeField] Text result;
	[SerializeField] PickupsController pickups;
	[SerializeField] Transform[] backgrounds = new Transform[4];

	private Rigidbody2D rb2d;
	private Vector2 goldenDifference;
	private int currentBackgroundIndex = 0;
	private int score = 0;
	private bool isAllowedToTrigger = true;
	private bool isInitial = true;
	private PositionRelativeToBackground currPositionRelativeToBG = PositionRelativeToBackground.DownRight;
	private string color = "";
	private bool canMove = true;

	private float tParam = 1f;

	void Start() {
		goldenDifference = new Vector2(backgrounds[1].position.x-backgrounds[0].position.x, backgrounds[0].position.y - backgrounds[2].position.y);
		rb2d = GetComponent<Rigidbody2D>();
		result.text = string.Format("Score: {0}", score);
	}

	void FixedUpdate() {
		//velocity
		float xVel = 0.98f*rb2d.velocity.x;
		float yVel = 0.98f*rb2d.velocity.y;
		rb2d.velocity = new Vector2(xVel, yVel);

		// movement
		if (canMove) {
			float moveHorizontal = Input.GetAxis("Horizontal");
			float moveVertical = Input.GetAxis("Vertical");
			Vector2 movement = new Vector2(moveHorizontal, moveVertical);
			rb2d.AddForce(movement*speed);
		}

		if (tParam < 1) {
			Expand();
		}
		// check the position of each polygon
		currentBackgroundIndex = FindBackgroundIndex();
		PositionRelativeToBackground newPositionRelativeToBG = FindPositionRelativeToBG();
		if (newPositionRelativeToBG != currPositionRelativeToBG) {
			currPositionRelativeToBG = newPositionRelativeToBG;
			switch (currPositionRelativeToBG) {
				case PositionRelativeToBackground.UpRight:
					TranslateBackground();
					break;
				case PositionRelativeToBackground.DownRight:
					TranslateBackground(false, true);
					break;
				case PositionRelativeToBackground.DownLeft:
					TranslateBackground(true, true);
					break;
				case PositionRelativeToBackground.UpLeft:
					TranslateBackground(true, false);
					break;
			}
		}
	}

	void Replace(GameObject gameObject) {
		// change the tag
		this.tag = gameObject.tag;

		// change the image
		var imageName = gameObject.GetComponent<SpriteRenderer>().sprite.name.Replace("Opponent", "You").Replace("4", "2");
		this.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(string.Format("Sprites/{0}", imageName));

    	// change the collider
    	Destroy(this.gameObject.GetComponent<Collider2D>());
		if (gameObject.GetComponent<Collider2D>().GetType() == typeof(CircleCollider2D)){
			CircleCollider2D cc = this.gameObject.AddComponent<CircleCollider2D>() as CircleCollider2D;
			cc.radius = 0.4f;
			cc.sharedMaterial = Resources.Load<PhysicsMaterial2D>("Physic_Material/line_material");
		} else if (gameObject.GetComponent<Collider2D>().GetType() == typeof(BoxCollider2D)) {
			BoxCollider2D bc = this.gameObject.AddComponent<BoxCollider2D>() as BoxCollider2D;
			bc.size = new Vector2(0.2f, 0.2f);
			bc.sharedMaterial = Resources.Load<PhysicsMaterial2D>("Physic_Material/line_material");
		} else if (gameObject.GetComponent<Collider2D>().GetType() == typeof(PolygonCollider2D)) {
			PolygonCollider2D pc = this.gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
			pc.sharedMaterial = Resources.Load<PhysicsMaterial2D>("Physic_Material/line_material");

		}
		tParam = 0;
	}

	void Expand() {
		float speed = 3f;
		float currScale = 0f;
		tParam += Time.deltaTime * speed;
		this.transform.localScale = new Vector3(Mathf.Lerp(0, 2, tParam), Mathf.Lerp(0, 2, tParam));
	}

    void OnTriggerEnter2D(Collider2D other) {
		if (!this.GetComponent<Collider2D>().bounds.Intersects(other.bounds) || !isAllowedToTrigger) {
			return;
		}
		isAllowedToTrigger = false;
		if (!isInitial && 
			this.tag != other.tag && 
			DetermineColor(other.GetComponent<SpriteRenderer>().sprite) != this.color) {
			result.text = "LOST";
			Debug.LogError(string.Format("other tag: {0}, this tag: {1}, other color: {2}, this color: {3}", other.tag, this.tag, DetermineColor(other.GetComponent<SpriteRenderer>().sprite), this.color));
			pickups.Stop();
			canMove = false;
			// call a function to: stop all the objects from moving, destroy this, and then display the main screen
			return;
		}
		isInitial = false;
		other.gameObject.GetComponent<AudioSource>().Play();
		color = DetermineColor(other.GetComponent<SpriteRenderer>().sprite);

		IncrementScore();
    	pickups.RemoveObj(other.transform);
		StartCoroutine(Decay(other.gameObject));
//		Decay(other.gameObject);
    }

    void IncrementScore() {
    	score++;
    	result.text = string.Format("Score: {0}", score);
    }

    string DetermineColor(Sprite sprite) {
    	if (sprite.name.Contains("Pink")) {
    		return "pink";
		} else if (sprite.name.Contains("Blue")) {
			return "blue";
		} else if (sprite.name.Contains("Orange")) {
			return "orange";
		} else {
			return "green";
    	}
    }

    IEnumerator Decay(GameObject other) {
    	var time = Time.time;
		float speed = 10f;
		float currScale = 0f;
		float timeParam = 0f;
		while(timeParam < 1) {
			timeParam += (Time.time - time) * speed;
			var offset = other.gameObject.transform.position - this.transform.position;

			// player
			this.transform.localScale = this.transform.localScale = new Vector3(Mathf.Lerp(2, 0, tParam), Mathf.Lerp(2, 0, tParam));
			this.rb2d.velocity = offset;

			// other
			this.transform.localScale = this.transform.localScale = new Vector3(Mathf.Lerp(2, 0, tParam), Mathf.Lerp(2, 0, tParam));
			other.GetComponent<Rigidbody2D>().velocity = offset * -1;

			yield return new WaitForSeconds(0.05f);
    	}
    	Replace(other);
    	Destroy(other);
    	isAllowedToTrigger = true;
    }

    int FindBackgroundIndex () {
    	for(int i=0; i<backgrounds.Length; i++) {
    		Vector2 offset = backgrounds[i].position - this.transform.position;
    		if(Mathf.Abs(offset.x) <= goldenDifference.x/2 && Mathf.Abs(offset.y) <= goldenDifference.y/2) {
    			return i;
    		}
    	}
    	return 0;
    }

    PositionRelativeToBackground FindPositionRelativeToBG() {
		Vector2 offset = this.transform.position - backgrounds[currentBackgroundIndex].position;
    	if (offset.x >= 0 && offset.y >= 0) { // UpperRight
    		return PositionRelativeToBackground.UpRight;
    	} else if (offset.x >= 0 && offset.y < 0) { // LowerRight
    		return PositionRelativeToBackground.DownRight;
    	} else if (offset.x <= 0 && offset.y < 0) { // LowerLeft
    		return PositionRelativeToBackground.DownLeft;
    	} else { // UpperLeft
    		return PositionRelativeToBackground.UpLeft;
    	}
    }

	void TranslateBackground(bool xNeg = false, bool yNeg = false) {
		var currX = backgrounds[currentBackgroundIndex].position.x;
		var currY = backgrounds[currentBackgroundIndex].position.y;
		var xVal = (xNeg) ? currX - goldenDifference.x : currX + goldenDifference.x;
		var yVal = (yNeg) ? currY - goldenDifference.y : currY + goldenDifference.y;
		backgrounds[(currentBackgroundIndex+1)%4].position = new Vector3(currX, yVal);
		backgrounds[(currentBackgroundIndex+2)%4].position = new Vector3(xVal, yVal);
		backgrounds[(currentBackgroundIndex+3)%4].position = new Vector3(xVal, currY);
	}
}
