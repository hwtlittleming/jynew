using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("Key", "Name", "Sex", "Level", "Exp", "Attack", "Speed", 
		"Defense", "Heal", "Strength", "IQ", "Constitution", "Agile", "Luck", "Moral", "IQ", "Skills", "Items", "Mp", "MaxMp", "MpType", "Hp", "MaxHp", "Hurt", "Weapon", "Armor", "Xiulianwupin", "CurrentSkill")]
	public class ES3UserType_RoleInstance : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_RoleInstance() : base(typeof(Jyx2.RoleInstance)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (Jyx2.RoleInstance)obj;
			
			writer.WriteProperty("Key", instance.Key, ES3Type_int.Instance);
			writer.WriteProperty("Name", instance.Name, ES3Type_string.Instance);
			writer.WriteProperty("Sex", instance.Sex, ES3Type_int.Instance);
			writer.WriteProperty("Level", instance.Level, ES3Type_int.Instance);
			writer.WriteProperty("Exp", instance.Exp, ES3Type_int.Instance);
			writer.WriteProperty("Attack", instance.Attack, ES3Type_int.Instance);
			writer.WriteProperty("Speed", instance.Speed, ES3Type_int.Instance);
			writer.WriteProperty("Defense", instance.Defense, ES3Type_int.Instance);
			writer.WriteProperty("Heal", instance.Heal, ES3Type_int.Instance);

			writer.WriteProperty("Strength", instance.Strength, ES3Type_int.Instance);
			writer.WriteProperty("IQ", instance.IQ, ES3Type_int.Instance);
			writer.WriteProperty("Constitution", instance.Constitution, ES3Type_int.Instance);
			writer.WriteProperty("Agile", instance.Agile, ES3Type_int.Instance);
			writer.WriteProperty("Luck", instance.Luck, ES3Type_int.Instance);

			writer.WriteProperty("Moral", instance.Moral, ES3Type_int.Instance);

			writer.WriteProperty("IQ", instance.IQ, ES3Type_int.Instance);

			writer.WriteProperty("Skills", instance.skills, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Jyx2.SkillInstance>)));
			writer.WriteProperty("Items", instance.Items, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Jyx2Configs.Jyx2ConfigCharacterItem>)));
			writer.WriteProperty("Mp", instance.Mp, ES3Type_int.Instance);
			writer.WriteProperty("MaxMp", instance.MaxMp, ES3Type_int.Instance);

			writer.WriteProperty("Hp", instance.Hp, ES3Type_int.Instance);
			writer.WriteProperty("MaxHp", instance.MaxHp, ES3Type_int.Instance);
			writer.WriteProperty("Hurt", instance.Hurt, ES3Type_int.Instance);

			writer.WriteProperty("Weapon", instance.Weapon, ES3Type_int.Instance);
			writer.WriteProperty("Armor", instance.Armor, ES3Type_int.Instance);
			writer.WriteProperty("Xiulianwupin", instance.Xiulianwupin, ES3Type_int.Instance);
			writer.WriteProperty("CurrentSkill", instance.CurrentSkill, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (Jyx2.RoleInstance)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "Key":
						instance.Key = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Name":
						instance.Name = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "Sex":
						instance.Sex = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Level":
						instance.Level = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Exp":
						instance.Exp = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Attack":
						instance.Attack = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Speed":
						instance.Speed = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Defense":
						instance.Defense = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Heal":
						instance.Heal = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Strength":
						instance.Strength = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "IQ":
						instance.IQ = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Constitution":
						instance.Constitution = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Agile":
						instance.Agile = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Luck":
						instance.Luck = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Moral":
						instance.Moral = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Skills":
						instance.skills = reader.Read<System.Collections.Generic.List<Jyx2.SkillInstance>>();
						break;
					case "Items":
						instance.Items = reader.Read<System.Collections.Generic.List<Jyx2Configs.Jyx2ConfigCharacterItem>>();
						break;
					case "Mp":
						instance.Mp = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "MaxMp":
						instance.MaxMp = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Hp":
						instance.Hp = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "MaxHp":
						instance.MaxHp = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Hurt":
						instance.Hurt = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Weapon":
						instance.Weapon = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Armor":
						instance.Armor = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "Xiulianwupin":
						instance.Xiulianwupin = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "CurrentSkill":
						instance.CurrentSkill = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new Jyx2.RoleInstance();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_RoleInstanceArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_RoleInstanceArray() : base(typeof(Jyx2.RoleInstance[]), ES3UserType_RoleInstance.Instance)
		{
			Instance = this;
		}
	}
}