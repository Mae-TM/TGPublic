using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityList : MonoBehaviour
{
	[SerializeField]
	private Button item;

	[SerializeField]
	private Text cooldown;

	[SerializeField]
	private Text cost;

	[SerializeField]
	private Text title;

	[SerializeField]
	private Text description;

	[SerializeField]
	private Text keybinding;

	private void OnEnable()
	{
		foreach (Transform item in item.transform.parent)
		{
			if (item != this.item.transform)
			{
				Object.Destroy(item.gameObject);
			}
		}
		HashSet<Attacking.Ability> hashSet = new HashSet<Attacking.Ability>();
		for (int i = 0; i < Player.player.abilities.Count; i++)
		{
			if (hashSet.Add(Player.player.abilities[i]))
			{
				if (i != 0)
				{
					this.item = Object.Instantiate(this.item, this.item.transform.parent);
				}
				AddAbility(this.item, Player.player.abilities[i], KeyboardControl.GetAbilityBinding(i));
			}
		}
		foreach (Player.ClasspectAbility effect in Player.player.GetEffects<Player.ClasspectAbility>())
		{
			if (hashSet.Add(effect))
			{
				AddAbility(Object.Instantiate(this.item, this.item.transform.parent), effect);
			}
		}
		foreach (Player.DecreasingStackEffect effect2 in Player.player.GetEffects<Player.DecreasingStackEffect>())
		{
			if (hashSet.Add(effect2.ability))
			{
				AddAbility(Object.Instantiate(this.item, this.item.transform.parent), effect2.ability);
			}
		}
	}

	private void AddAbility(Button item, Attacking.Ability ability, InputAction input = null)
	{
		item.GetComponentInChildren<Text>().text = ">" + ability.name;
		item.GetComponentInChildren<Text>().color = ability.color;
		Color.RGBToHSV(ability.color, out var H, out var S, out var V);
		Material material = new Material(item.image.material);
		material.SetFloat("_HueShift", H * 360f);
		material.SetFloat("_Sat", S);
		material.SetFloat("_Val", V);
		item.image.material = material;
		item.name = "Ability " + ability.name;
		item.onClick.RemoveAllListeners();
		item.onClick.AddListener(delegate
		{
			if (input == null)
			{
				cooldown.transform.parent.gameObject.SetActive(value: false);
				cost.transform.parent.gameObject.SetActive(value: false);
				keybinding.gameObject.SetActive(value: false);
			}
			else
			{
				cooldown.text = Sylladex.MetricFormat(ability.maxCooldown);
				cost.text = Sylladex.MetricFormat(ability.vimcost);
				keybinding.text = input.controls[0].displayName;
				cooldown.transform.parent.gameObject.SetActive(value: true);
				cost.transform.parent.gameObject.SetActive(ability.vimcost != 0f);
				keybinding.gameObject.SetActive(value: true);
			}
			title.text = ability.name;
			description.text = ability.description;
			title.gameObject.SetActive(value: true);
			description.gameObject.SetActive(value: true);
		});
	}
}
