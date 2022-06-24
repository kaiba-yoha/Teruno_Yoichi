using Kaiba;
using Photon.Pun;
using UnityEngine;

namespace Kaiba.Teruno_System
{

    public class ArrowController : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private bool isLanched = false;
        [SerializeField] private bool isSet = false;
        [SerializeField] public BowController _bowManager;
        [SerializeField] private float ForceCoefficient = 1f;
        Rigidbody rb;
        private float dis = 0;

        [SerializeField]
        GameObject hitEffectPrefab;

        [SerializeField]
        PhotonView view;
        

        // Start is called before the first frame update
        void Start()
        {
            _bowManager = GameObject.Find("BowManager").GetComponent<BowController>();
            rb = this.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Update is called once per frame
        void Update()
        {
            if (!TerunoManager.IsHost)
                return;

            if (isSet)
            {
                dis = (_bowManager._LeftHand.transform.position - _bowManager._RightHand.transform.position).magnitude;
                _bowManager.BowDrawing(dis);
                Vector3 localPos = this.transform.localPosition;
                localPos.z = -dis;
                this.transform.localPosition = localPos;
            }

            if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) && !isLanched)
            {
                Shot(dis);
            }

            if (transform.position.magnitude > 200) Network.NetworkUtility.DestroyNetworkObject(gameObject);
        }

        void ShotRPC(float distance)
        {
            view.RPC("Shot", RpcTarget.All, new object[] { distance });
        }

        [PunRPC]
        public void Shot(float distance)
        {
            isSet = false;
            isLanched = true;
            this.transform.parent = null;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(this.transform.forward * ForceCoefficient * distance, ForceMode.Impulse);
            _bowManager.arrow_num++;
            _bowManager.Reset();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (!TerunoManager.IsHost)
                return;

            if (other.gameObject.tag == "Bow" && !isLanched)
            {
                isSet = true;
                rb.constraints = RigidbodyConstraints.None;
                this.transform.parent = other.transform;
                this.transform.localPosition = other.transform.localPosition;
                this.transform.localRotation = Quaternion.identity;
                rb.constraints = RigidbodyConstraints.FreezePositionY;
                rb.constraints = RigidbodyConstraints.FreezePositionZ;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (other.gameObject.tag == "Targets")
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                OnHit();
            }
        }

        public void OnHit()
        {
            view.RPC("HitRPC", RpcTarget.All, new object[] { });
        }

        [PunRPC]
        public void HitRPC()
        {
            Instantiate(hitEffectPrefab, transform.position + transform.forward * 0.05f, transform.rotation);
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

        }
    }

}