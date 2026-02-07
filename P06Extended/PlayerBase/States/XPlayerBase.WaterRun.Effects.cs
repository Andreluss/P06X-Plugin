namespace P06X
{
    using UnityEngine;
    using Helpers;

    public partial class XPlayerBase : MonoBehaviour
    {
        // Cached water run particle prefabs (cloned from SonicEffects)
        private ParticleSystem[] _waterRunFX;

        /// <summary>
        /// Lazily loads and clones the waterRun particles from SonicEffects,
        /// then parents them to the current player so any character can use them.
        /// </summary>
        public ParticleSystem[] WaterRunFX
        {
            get
            {
                if (_waterRunFX != null && _waterRunFX.Length > 0 && _waterRunFX[0] != null)
                    return _waterRunFX;

                // Try to find a SonicNew instance first (may not exist for non-Sonic characters)
                SonicNew sonicNew = XSingleton<XDebug>.Instance.SonicNew;

                // Fallback: instantiate Sonic prefab temporarily to grab the effects
                ParticleSystem[] sourceParticles = null;
                GameObject tempPrefab = null;

                if (sonicNew != null)
                {
                    sourceParticles = sonicNew.SonicEffects.waterRun;
                }
                else
                {
                    // Load the Sonic prefab from Resources to extract the particle systems
                    GameObject prefab = Resources.Load<GameObject>("DefaultPrefabs/Player/sonic_new");
                    if (prefab != null)
                    {
                        tempPrefab = Object.Instantiate(prefab);
                        tempPrefab.SetActive(false);
                        SonicNew tempSonic = tempPrefab.GetComponent<SonicNew>();
                        if (tempSonic != null && tempSonic.SonicEffects != null)
                        {
                            sourceParticles = tempSonic.SonicEffects.waterRun;
                        }
                    }
                }

                if (sourceParticles != null && sourceParticles.Length > 0)
                {
                    _waterRunFX = new ParticleSystem[sourceParticles.Length];
                    for (int i = 0; i < sourceParticles.Length; i++)
                    {
                        GameObject clone = Object.Instantiate(sourceParticles[i].gameObject, I.I.transform);
                        clone.transform.localPosition = Vector3.zero;
                        clone.transform.localRotation = Quaternion.identity;
                        ParticleSystem ps = clone.GetComponent<ParticleSystem>();
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        _waterRunFX[i] = ps;
                    }
                }
                else
                {
                    Debug.LogWarning("[XPlayerBase] Could not find waterRun particles from SonicEffects.");
                    _waterRunFX = new ParticleSystem[0];
                }

                if (tempPrefab != null)
                {
                    Object.Destroy(tempPrefab);
                }

                return _waterRunFX;
            }
        }

        public void PlayWaterRunFX()
        {
            foreach (ParticleSystem ps in WaterRunFX)
            {
                if (ps != null && !ps.isPlaying)
                    ps.Play();
            }
        }

        public void StopWaterRunFX()
        {
            if (_waterRunFX == null) return;
            foreach (ParticleSystem ps in _waterRunFX)
            {
                if (ps != null && ps.isPlaying)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}