using UnityEngine;

public class ShootBehaviour : StateMachineBehaviour
{
	public float bulletSpeed;

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Attacking component = animator.transform.parent.GetComponent<Attacking>();
		Bullet bullet = component.GetBullet();
		if (bullet.HasTag(NormalItem.Tag.Scatter))
		{
			int num = Mathf.RoundToInt(3f / (float)bullet.GetTagCount());
			float damagePortion = 1f / (float)(num + 1);
			Collider[] array = new Collider[num + 1];
			array[0] = Bullet.Make(component, bullet, bulletSpeed, damagePortion);
			for (int i = 0; i < num; i++)
			{
				float angle = 10f * (float)(2 * i + 1 - num);
				array[i + 1] = Bullet.Make(component, bullet, bulletSpeed, damagePortion, angle);
				for (int j = 0; j <= i; j++)
				{
					Physics.IgnoreCollision(array[i + 1], array[j]);
				}
			}
		}
		else
		{
			Bullet.Make(component, bullet, bulletSpeed);
		}
	}
}
