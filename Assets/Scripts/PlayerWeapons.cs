using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapons : MonoBehaviour
{
    [Header("Weapon Particle Systems")]
    [Tooltip("Ateş eden lazer sistemleri (Laser_L ve Laser_R)")]
    [SerializeField] private ParticleSystem[] lasers;

    [Header("Aim & Target Cubes")]
    [Tooltip("Lazerlerin takip edeceği Cube objeleri. Boş bırakılırsa lazerlerin içindeki Cube objeleri otomatik bulunur.")]
    [SerializeField] private Transform[] targetCubes;

    [Tooltip("Kamera referansı (Boş bırakılırsa otomatik bulunur)")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Hedef küplerin geminin önündeki sabit mesafesi (Z ekseni sabit kalır)")]
    [SerializeField] private float aimDistance = 25f;

    [Tooltip("Fareyi takip etme yumuşaklığı (0 verilirse anında takip eder)")]
    [SerializeField] private float aimSmoothSpeed = 25f;

    [Tooltip("True ise iki lazer farenin hedeflediği tek noktada birleşir (Crosshair). False ise paralel kalırlar.")]
    [SerializeField] private bool convergeAtCrosshair = true;

    [Tooltip("Oyun esnasında hedef küplerin görünmesini istemiyorsanız işaretleyin.")]
    [SerializeField] private bool hideCubesInGame = false;

    private bool isFiring = false;

    void Start()
    {
        InitializeCamera();
        InitializeTargetCubes();
    }

    void Update()
    {
        UpdateCubePositionsAndLaserAim();
        HandleInput();
    }

    private void InitializeCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
    }

    private void InitializeTargetCubes()
    {
        // Eğer targetCubes Inspector'dan atanmamışsa, lasers dizisindeki objelerin içinde Cube ara
        if ((targetCubes == null || targetCubes.Length == 0) && lasers != null)
        {
            List<Transform> foundCubes = new List<Transform>();

            foreach (var laser in lasers)
            {
                if (laser == null) continue;

                Transform cube = laser.transform.Find("Cube");
                if (cube == null)
                {
                    // İsmi küçük/büyük harf veya "Cube" içeren alt objeyi ara
                    foreach (Transform child in laser.transform)
                    {
                        if (child.name.IndexOf("cube", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            cube = child;
                            break;
                        }
                    }
                }

                // Eğer isimle bulunamadıysa ilk alt objeyi al
                if (cube == null && laser.transform.childCount > 0)
                {
                    cube = laser.transform.GetChild(0);
                }

                if (cube != null)
                {
                    foundCubes.Add(cube);
                }
            }

            targetCubes = foundCubes.ToArray();
        }

        // Küpleri hazırla
        if (targetCubes != null)
        {
            foreach (var cube in targetCubes)
            {
                if (cube == null) continue;

                // 1. Collider'ı kapat: Mermilerin/lazerlerin veya geminin hedef küpe çarpmasını engelle
                Collider col = cube.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                // 2. İsteğe bağlı olarak görseli gizle
                if (hideCubesInGame)
                {
                    Renderer rend = cube.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.enabled = false;
                    }
                }

                // 3. Küpü lazerin içinden çıkarıp doğrudan gemiye (bu objeye) bağla
                // Bu sayede lazer küpe doğru dönerken küp de onunla birlikte dönüp titreşme (jitter) yapmaz
                cube.SetParent(this.transform, true);
            }
        }
    }

    private void UpdateCubePositionsAndLaserAim()
    {
        if (mainCamera == null || lasers == null || lasers.Length == 0) return;

        // 1. Farenin ekran konumunu al
        Vector2 mouseScreenPos;
        if (Mouse.current != null)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
        }
        else
        {
            mouseScreenPos = Input.mousePosition;
        }

        // 2. Kameradan fareye doğru bir Ray oluştur
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        // Farenin baktığı dünya konumunu al
        Vector3 worldAimPoint = ray.GetPoint(aimDistance);

        // Bu noktayı geminin (Model) yerel koordinat sistemine çevir
        Vector3 localAimPoint = transform.InverseTransformPoint(worldAimPoint);

        // 3. Küpleri sadece X ve Y eksenlerinde hareket ettir & Lazerleri küplere çevir
        for (int i = 0; i < lasers.Length; i++)
        {
            var laser = lasers[i];
            if (laser == null) continue;

            Transform cube = null;
            if (targetCubes != null && targetCubes.Length > 0)
            {
                if (i < targetCubes.Length && targetCubes[i] != null)
                {
                    cube = targetCubes[i];
                }
                else
                {
                    cube = targetCubes[0]; // Tek bir küp varsa her iki lazer de onu kullanır
                }
            }
            //Test

            if (cube == null) continue;

            // X ve Y değerlerini hesapla
            float targetLocalX = localAimPoint.x;
            float targetLocalY = localAimPoint.y;

            // Eğer lazerler paralel kalsın isteniyorsa, lazerin kendi X ofsetini koru
            if (!convergeAtCrosshair)
            {
                Vector3 laserLocalPos = transform.InverseTransformPoint(laser.transform.position);
                targetLocalX += laserLocalPos.x;
            }

            // Z ekseni KESİNLİKLE SABİT kalır!
            float targetLocalZ = aimDistance;

            Vector3 targetLocalPos = new Vector3(targetLocalX, targetLocalY, targetLocalZ);

            // Yumuşak geçişle (Smooth) veya anında konumu uygula
            if (aimSmoothSpeed > 0f)
            {
                cube.localPosition = Vector3.Lerp(cube.localPosition, targetLocalPos, Time.deltaTime * aimSmoothSpeed);
            }
            else
            {
                cube.localPosition = targetLocalPos;
            }

            // 4. Lazerin kendisi küpe baksın
            Vector3 directionToCube = cube.position - laser.transform.position;
            if (directionToCube != Vector3.zero)
            {
                laser.transform.rotation = Quaternion.LookRotation(directionToCube, transform.up);
            }
        }
    }

    private void HandleInput()
    {
        bool mouseFiring = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool spaceFiring = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool legacyMouseFiring = Input.GetMouseButton(0);

        if (mouseFiring || spaceFiring || legacyMouseFiring)
        {
            if (!isFiring)
            {
                isFiring = true;
                SetLaserEmission(true);
            }
        }
        else
        {
            if (isFiring)
            {
                isFiring = false;
                SetLaserEmission(false);
            }
        }
    }

    private void SetLaserEmission(bool active)
    {
        if (lasers == null) return;

        foreach (var laser in lasers)
        {
            if (laser == null) continue;

            var emission = laser.emission;
            emission.enabled = active;

            if (active && !laser.isPlaying)
            {
                laser.Play();
            }
            else if (!active && laser.isPlaying)
            {
                laser.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (lasers == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < lasers.Length; i++)
        {
            if (lasers[i] == null) continue;

            Transform cube = null;
            if (targetCubes != null && i < targetCubes.Length)
            {
                cube = targetCubes[i];
            }

            if (cube != null)
            {
                Gizmos.DrawLine(lasers[i].transform.position, cube.position);
                Gizmos.DrawWireCube(cube.position, cube.localScale);
            }
        }
    }
}