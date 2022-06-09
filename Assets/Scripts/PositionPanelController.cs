using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PositionPanelController : MonoBehaviour
{
    [SerializeField]
    private SpaceshipController spaceship;

    [SerializeField]
    private Text numberText;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Image indicator;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Sprite backgroundPlayer;

    [SerializeField]
    private float yStart;

    [SerializeField]
    private float yDelta;

    private bool isAnimating;

    private void Start()
    {
        indicator.color = spaceship.GetComponent<SpriteRenderer>().color;

        if (spaceship.isPlayer)
        {
            nameText.text = "You";
            background.sprite = backgroundPlayer;
        } else {
            nameText.text = spaceship.name;
        }
    }

    private void LateUpdate()
    {
            numberText.text = spaceship.currentPosition.ToString();
            if (!isAnimating) {
                float targetY = yStart + yDelta * (spaceship.currentPosition - 1);
                StartCoroutine(AnimatePanelVertically(targetY, 0.5f));
            }
    }

    public IEnumerator AnimatePanelVertically(float target, float duration = 1f)
    {
        isAnimating = true;
        float startY = transform.localPosition.y;
        float curTime = 0f;
        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            float curY = Mathf.SmoothStep(startY, target, curTime / duration);
            transform.localPosition = new Vector2(transform.localPosition.x, curY);
            yield return null;
        }

        isAnimating = false;
    }
}
