using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleTest : MonoBehaviour {

	private ParticleSystem particle;
	public Transform body;
	public Transform hand;


	private Vector3 bodyEffect = Vector3.zero;
	private Vector3 armEffect = Vector3.zero;

	public bool focus = false;

	private ParticleSystem.Particle[] particles;

	//粒子发射器绑定在角色手上，卷尘的粒子效果

	void Awake(){
		particle = this.GetComponent<ParticleSystem>();
	}


	void FixedUpdate()
	{
		
		MoveWithBody();
		FloatWithArm();
	  ApplyEffect();

		ActionWithHand();
		ApplyAction();

		//用身体的相对位置来直接改变pos
		//用右手离肩的距离添加额外速度
		//伸出手指或握拳使粒子聚集


	}

	public void MoveWithBody(){
		bodyEffect = new Vector3(this.body.position.x*3,0,this.body.position.z*3);
	}

	public void FloatWithArm(){
		KinectInterop.JointType handJoint = KinectInterop.JointType.HandRight;
		KinectInterop.JointType shoulderJoint = KinectInterop.JointType.ShoulderRight;
		KinectManager manager = KinectManager.Instance;
		if(manager && manager.IsInitialized()){
			if(manager.IsUserDetected()){
				long userId = manager.GetPrimaryUserID();
				Vector3 handPos,shoulderPos;
				if(manager.IsJointTracked(userId,(int)shoulderJoint)){
					shoulderPos = manager.GetJointPosition(userId,(int)shoulderJoint);
					if(manager.IsJointTracked(userId,(int)handJoint)){
						handPos = manager.GetJointPosition(userId,(int)handJoint);
						armEffect = new Vector3(-3*(handPos.x-shoulderPos.x),3*(handPos.y-shoulderPos.y),-3*(handPos.z-shoulderPos.z));
						ParticleSystem.VelocityOverLifetimeModule module = particle.velocityOverLifetime;
					}
				}
			}
		}
	}

	public void ActionWithHand(){
		KinectInterop.JointType handJoint = KinectInterop.JointType.HandRight;
		KinectManager manager = KinectManager.Instance;
		if(manager && manager.IsInitialized()){
			if(manager.IsUserDetected()){
				long userId = manager.GetPrimaryUserID();
				if(manager.IsJointTracked(userId,(int)handJoint)){
					KinectInterop.HandState handState = manager.GetRightHandState(userId);
					if(handState == KinectInterop.HandState.Closed){
						focus = true;
					} else if(handState == KinectInterop.HandState.Open){
						focus = false;
					}
				}
			}
		}
	}

	public void ApplyEffect(){
		ParticleSystem.VelocityOverLifetimeModule module = particle.velocityOverLifetime;

		ParticleSystem.MinMaxCurve curveX = new ParticleSystem.MinMaxCurve();
		ParticleSystem.MinMaxCurve curveY = new ParticleSystem.MinMaxCurve();
		ParticleSystem.MinMaxCurve curveZ = new ParticleSystem.MinMaxCurve();

		curveX.constant = bodyEffect.x + armEffect.x;
		curveY.constant = bodyEffect.y + armEffect.y;
		curveZ.constant = bodyEffect.z + armEffect.z;

		module.x = curveX;
		module.y = curveY;
		module.z = curveZ;
	}

	public void ApplyAction(){
		if(!focus) return;
		if(particles==null || particles.Length<particle.main.maxParticles){
			particles = new ParticleSystem.Particle[particle.main.maxParticles];
		}
		int num = particle.GetParticles(particles);

		for(int i=0;i<num;i++){
			particles[i].position = Vector3.MoveTowards(particles[i].position,this.hand.position,0.06f);

		}
		particle.SetParticles(particles,num);
	}


}



