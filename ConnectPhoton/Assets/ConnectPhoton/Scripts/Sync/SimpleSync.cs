using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SimpleSync : MonoBehaviourPun, IPunObservable
{
    [SerializeField] bool syncPosition = true;
    [SerializeField] bool syncRotation = false;
    [SerializeField] bool useSmoothing = true;
    [SerializeField] float smoothingSpeed = 130f;
    Vector3 networkPosition;
    Quaternion networkRotation;

    private void Update()
    {
        if (!useSmoothing || photonView.IsMine) return;
        if (syncPosition) transform.position = Vector3.MoveTowards(transform.localPosition, networkPosition, smoothingSpeed * Vector3.Distance(transform.position, networkPosition) * Time.deltaTime * (1.0f / PhotonNetwork.SerializationRate));
        if (syncRotation) transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, smoothingSpeed * Quaternion.Angle(transform.rotation, networkRotation) * Time.deltaTime * (1.0f / PhotonNetwork.SerializationRate));
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // If this is owned by the client, we write data for all the other clients to receive
        if (stream.IsWriting)
        {
            if (syncPosition) stream.SendNext(transform.position);
            if (syncRotation) stream.SendNext(transform.rotation);
        }
        // If this isn't owned by the client we read data instead of writing
        else if(stream.IsReading)
        {
            if (syncPosition)
            {
                if(useSmoothing) networkPosition = (Vector3)stream.ReceiveNext();
                else transform.position = (Vector3)stream.ReceiveNext();
            }
            if (syncRotation)
            {
                if(useSmoothing) networkRotation = (Quaternion)stream.ReceiveNext();
                else transform.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
