using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DisallowMultipleComponent]
[RequireComponent(typeof(XRGrabInteractable))]
public class CangkulPintar : MonoBehaviour
{
    [Header("Tanah")]
    public GameObject prefabTanah;
    [Min(0.05f)] public float ukuranGrid = 0.5f;
    [Min(1)] public int butuhBerapaPukulan = 1;
    [Min(0f)] public float jedaAntarpukulan = 0.2f;

    [Header("Haptics")]
    [Range(0f, 1f)] public float kekuatanGetaran = 0.7f;
    [Min(0f)] public float durasiGetaran = 0.15f;

    [Header("Efek Benturan (Opsional)")]
    [Tooltip("Jika kosong, sistem membuat partikel debu sederhana saat runtime.")]
    public ParticleSystem partikelBenturan;
    [Min(1)] public int jumlahPartikel = 14;
    public Color warnaDebu = new Color(0.42f, 0.27f, 0.12f, 0.9f);
    [Tooltip("Jika kosong, sistem membuat AudioSource 3D saat runtime.")]
    public AudioSource sumberSuara;
    [Tooltip("Jika kosong, sistem membuat suara benturan pendek secara prosedural.")]
    public AudioClip suaraBenturan;
    [Range(0f, 1f)] public float volumeBenturan = 0.8f;
    [Range(0f, 0.3f)] public float variasiPitch = 0.08f;

    private readonly Dictionary<Vector2Int, int> hitCounts = new Dictionary<Vector2Int, int>();
    private readonly Dictionary<Vector2Int, float> lastHitTimes = new Dictionary<Vector2Int, float>();
    private XRGrabInteractable grabInteractable;
    private int tanahLayer;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        tanahLayer = LayerMask.NameToLayer("Tanah");
        SiapkanEfekBenturan();
    }

    private void OnValidate()
    {
        ukuranGrid = Mathf.Max(0.05f, ukuranGrid);
        butuhBerapaPukulan = Mathf.Max(1, butuhBerapaPukulan);
        jedaAntarpukulan = Mathf.Max(0f, jedaAntarpukulan);
    }

    private void OnTriggerEnter(Collider bendaKena)
    {
        if (CobaCangkulPlot(bendaKena))
            return;

        Vector3 titikBenturan = bendaKena.ClosestPoint(transform.position);
        CekTabrakan(bendaKena.gameObject, titikBenturan, Vector3.up);
    }

    private void OnCollisionEnter(Collision tabrakan)
    {
        if (CobaCangkulPlot(tabrakan.collider))
            return;

        ContactPoint contact = tabrakan.contactCount > 0
            ? tabrakan.GetContact(0)
            : new ContactPoint();

        Vector3 titikBenturan = tabrakan.contactCount > 0 ? contact.point : transform.position;
        Vector3 normalBenturan = tabrakan.contactCount > 0 ? contact.normal : Vector3.up;
        CekTabrakan(tabrakan.gameObject, titikBenturan, normalBenturan);
    }

    private bool CobaCangkulPlot(Collider colliderKena)
    {
        TanahBerkebun plot = colliderKena.GetComponentInParent<TanahBerkebun>();
        if (plot == null)
            return false;

        bool berubah = plot.Dicangkul();
        if (berubah)
        {
            Vector3 titikBenturan = colliderKena.ClosestPoint(transform.position);
            MainkanEfekBenturan(titikBenturan, plot.transform.up);
            BeriGetaranKeTangan();
        }

        return true;
    }

    private void CekTabrakan(GameObject bendaKena, Vector3 titikBenturan, Vector3 normalBenturan)
    {
        if (tanahLayer < 0 || bendaKena.layer != tanahLayer || prefabTanah == null)
            return;

        Vector2Int gridKey = new Vector2Int(
            Mathf.RoundToInt(titikBenturan.x / ukuranGrid),
            Mathf.RoundToInt(titikBenturan.z / ukuranGrid));

        if (lastHitTimes.TryGetValue(gridKey, out float lastHitTime) &&
            Time.time - lastHitTime < jedaAntarpukulan)
        {
            return;
        }

        lastHitTimes[gridKey] = Time.time;
        DapatkanPermukaanTanah(gridKey, titikBenturan, out Vector3 titikTanam, out Vector3 normalTanah);

        if (SudahAdaTanahBerkebun(titikTanam))
        {
            BeriGetaranKeTangan(0.3f, 0.1f);
            return;
        }

        int jumlahPukulan = hitCounts.TryGetValue(gridKey, out int hitCount) ? hitCount + 1 : 1;
        hitCounts[gridKey] = jumlahPukulan;

        Vector3 arahEfek = normalTanah.sqrMagnitude > 0.01f ? normalTanah : normalBenturan;
        MainkanEfekBenturan(titikBenturan, arahEfek);
        BeriGetaranKeTangan();

        Debug.Log($"Cangkul memukul grid {gridKey} sebanyak {jumlahPukulan}/{butuhBerapaPukulan} kali.");

        if (jumlahPukulan < butuhBerapaPukulan)
            return;

        Quaternion rotasiTanah = Quaternion.FromToRotation(Vector3.up, normalTanah) * prefabTanah.transform.rotation;
        Instantiate(prefabTanah, titikTanam, rotasiTanah);
        hitCounts.Remove(gridKey);
        lastHitTimes.Remove(gridKey);
    }

    private void DapatkanPermukaanTanah(
        Vector2Int gridKey,
        Vector3 titikBenturan,
        out Vector3 titikTanam,
        out Vector3 normalTanah)
    {
        float x = gridKey.x * ukuranGrid;
        float z = gridKey.y * ukuranGrid;
        Terrain terrain = Terrain.activeTerrain;

        if (terrain != null)
        {
            Vector3 localPoint = new Vector3(x, 0f, z) - terrain.transform.position;
            Vector3 terrainSize = terrain.terrainData.size;

            if (localPoint.x >= 0f && localPoint.x <= terrainSize.x &&
                localPoint.z >= 0f && localPoint.z <= terrainSize.z)
            {
                float normalizedX = localPoint.x / terrainSize.x;
                float normalizedZ = localPoint.z / terrainSize.z;
                float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
                titikTanam = new Vector3(x, y, z);
                normalTanah = terrain.terrainData.GetInterpolatedNormal(normalizedX, normalizedZ).normalized;
                return;
            }
        }

        int layerMask = 1 << tanahLayer;
        Vector3 rayOrigin = new Vector3(x, titikBenturan.y + 2f, z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5f, layerMask, QueryTriggerInteraction.Collide))
        {
            titikTanam = hit.point;
            normalTanah = hit.normal.normalized;
            return;
        }

        titikTanam = new Vector3(x, titikBenturan.y, z);
        normalTanah = Vector3.up;
    }

    private bool SudahAdaTanahBerkebun(Vector3 titikTanam)
    {
        float radius = Mathf.Max(0.1f, ukuranGrid * 0.2f);
        Collider[] colliders = Physics.OverlapSphere(titikTanam, radius);

        foreach (Collider col in colliders)
        {
            if (col.GetComponentInParent<TanahBerkebun>() != null)
                return true;
        }

        return false;
    }

    private void BeriGetaranKeTangan(float customAmplitude = -1f, float customDuration = -1f)
    {
        if (grabInteractable == null || !grabInteractable.isSelected)
            return;

        if (grabInteractable.firstInteractorSelecting is XRBaseInputInteractor inputInteractor)
        {
            float amplitude = customAmplitude >= 0f ? customAmplitude : kekuatanGetaran;
            float duration = customDuration >= 0f ? customDuration : durasiGetaran;
            inputInteractor.SendHapticImpulse(amplitude, duration);
        }
    }

    private void SiapkanEfekBenturan()
    {
        if (partikelBenturan == null)
        {
            GameObject particleObject = new GameObject("Cangkul Impact Dust");
            particleObject.transform.SetParent(transform, false);
            partikelBenturan = particleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = partikelBenturan.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.gravityModifier = 0.8f;
            main.maxParticles = 80;

            ParticleSystem.EmissionModule emission = partikelBenturan.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = partikelBenturan.shape;
            shape.enabled = false;
            partikelBenturan.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (sumberSuara == null)
        {
            sumberSuara = gameObject.AddComponent<AudioSource>();
            sumberSuara.playOnAwake = false;
            sumberSuara.spatialBlend = 1f;
            sumberSuara.minDistance = 1f;
            sumberSuara.maxDistance = 12f;
            sumberSuara.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        if (suaraBenturan == null)
            suaraBenturan = BuatSuaraBenturanProsedural();
    }

    private void MainkanEfekBenturan(Vector3 posisi, Vector3 normal)
    {
        normal = normal.sqrMagnitude > 0.01f ? normal.normalized : Vector3.up;

        if (partikelBenturan != null)
        {
            for (int i = 0; i < jumlahPartikel; i++)
            {
                Vector3 arahAcak = Random.insideUnitSphere * 0.65f;
                if (Vector3.Dot(arahAcak, normal) < 0f)
                    arahAcak = Vector3.Reflect(arahAcak, normal);

                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
                {
                    position = posisi + normal * 0.015f,
                    velocity = normal * Random.Range(0.35f, 0.85f) + arahAcak,
                    startColor = warnaDebu,
                    startSize = Random.Range(0.04f, 0.1f)
                };
                partikelBenturan.Emit(emitParams, 1);
            }
        }

        if (sumberSuara != null && suaraBenturan != null)
        {
            sumberSuara.pitch = 1f + Random.Range(-variasiPitch, variasiPitch);
            sumberSuara.PlayOneShot(suaraBenturan, volumeBenturan);
        }
    }

    private static AudioClip BuatSuaraBenturanProsedural()
    {
        const int sampleRate = 44100;
        const float duration = 0.18f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        System.Random random = new System.Random(7319);

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float envelope = Mathf.Pow(1f - i / (float)sampleCount, 3f);
            float noise = ((float)random.NextDouble() * 2f - 1f) * 0.55f;
            float thump = Mathf.Sin(2f * Mathf.PI * 95f * time) * 0.45f;
            samples[i] = (noise + thump) * envelope;
        }

        AudioClip clip = AudioClip.Create("Procedural Hoe Impact", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
