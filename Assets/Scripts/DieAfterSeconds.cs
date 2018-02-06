using UnityEngine;
using System.Collections;

public class DieAfterSeconds : MonoBehaviour {
    IEnumerator Start () {
        // Careful - make this time too long, and the stars disappear before the poofs destroy themselves
        // leaving extraneous child poofs when stars reappear
        // need a way for the stars to disappear before poofs, but still be active for a bit
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
	}
}
