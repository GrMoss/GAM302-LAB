using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private float fakeLoadDuration = 3f;
    [SerializeField] private float loadCompletionSpeed = 0.5f;

    public static bool continueLoading = false;

    private void Awake()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
        }

        if (percentText != null)
        {
            percentText.text = "0%";
        }
    }

    private void Start()
    {
        StartCoroutine(FakeLoadProcess());
    }

    public void ContinueLoading()
    {
        continueLoading = true;
    }

    private IEnumerator FakeLoadProcess()
    {
        int pauseCount = Random.Range(2, 5);
        float[] pausePoints = new float[pauseCount];
        float segmentDuration = fakeLoadDuration / (pauseCount + 1);

        for (int i = 0; i < pauseCount; i++)
        {
            float segmentStart = i * segmentDuration;
            float segmentEnd = (i + 1) * segmentDuration;
            pausePoints[i] = Random.Range(segmentStart, segmentEnd);
        }

        float elapsedTime = 0f;
        float currentProgress = 0f;
        int currentPauseIndex = 0;
        float partialTarget = Random.Range(0.4f, 0.7f);

        while (elapsedTime < fakeLoadDuration && currentProgress < partialTarget)
        {
            elapsedTime += Time.deltaTime;
            float targetProgress = Mathf.Clamp01(elapsedTime / fakeLoadDuration) * partialTarget / 1f;

            if (currentPauseIndex < pauseCount && Mathf.Approximately(currentProgress, Mathf.Clamp01(pausePoints[currentPauseIndex] / fakeLoadDuration * partialTarget)))
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
                currentPauseIndex++;
            }
            else
            {
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);
            }

            if (loadingSlider != null)
            {
                loadingSlider.value = currentProgress;
            }
            if (percentText != null)
            {
                percentText.text = Mathf.RoundToInt(currentProgress * 100) + "%";
            }
            yield return null;
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = partialTarget;
        }
        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(partialTarget * 100) + "%";
        }

        yield return new WaitUntil(() => continueLoading);

        while (currentProgress < 1f)
        {
            if (currentPauseIndex < pauseCount && Mathf.Approximately(currentProgress, Mathf.Clamp01(pausePoints[currentPauseIndex] / fakeLoadDuration)))
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
                currentPauseIndex++;
            }
            else
            {
                currentProgress = Mathf.MoveTowards(currentProgress, 1f, Time.deltaTime * loadCompletionSpeed);
            }

            if (loadingSlider != null)
            {
                loadingSlider.value = currentProgress;
            }
            if (percentText != null)
            {
                percentText.text = Mathf.RoundToInt(currentProgress * 100) + "%";
            }
            yield return null;
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 1f;
        }
        if (percentText != null)
        {
            percentText.text = "100%";
        }

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
    }
}