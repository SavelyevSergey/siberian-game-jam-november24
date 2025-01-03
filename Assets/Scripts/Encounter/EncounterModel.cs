using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Skills;
using Unity.VisualScripting;
using UnityEngine;

namespace Encounter
{
    [CreateAssetMenu(menuName = "Game/Models/Encounter", fileName = "Encounter")]
    public class EncounterModel : ScriptableObject
    {
        [Title("Encounter")]
        [SerializeField] private string _id;
        [Title("Hero")]
        [SerializeField] private EntityModel _heroEntity;
        [SerializeField] private List<SkillForEncounterModel> _heroSkillOrder;
        [Title("Player")]
        [SerializeField] private List<SkillModel> _playerSkills;
        [Title("Enemies")]
        [OnCollectionChanged(Before = "BeforeEnemyChange", After = "AfterEnemyChange")]
        [SerializeField] private List<EntityForEncounterModel> _enemies;
        [Title(" ")]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        [SerializeField] private List<EntityPointer> _turnOrder;

        public string Id => _id;
        public EntityModel Hero => _heroEntity;
        public List<SkillForEncounterModel> HeroSkillOrder => _heroSkillOrder;
        public List<SkillModel> PlayerSkills => _playerSkills;
        public List<EntityForEncounterModel> Enemies => _enemies;
        public List<string> StepOrder => _turnOrder.Select(t => t.Id).ToList();

#if UNITY_EDITOR
        private void Reset()
        {
            ResetTurnOrder();
        }
        
        private void BeforeEnemyChange(CollectionChangeInfo change)
        {
            switch (change.ChangeType)
            {
                case CollectionChangeType.RemoveIndex:
                    var id = _enemies[change.Index].Id;
                    var idx = _turnOrder.FindIndex(t => t.Id == id);
                    _turnOrder.RemoveAt(idx);
                    break;
                case CollectionChangeType.Clear:
                    ResetTurnOrder();
                    break;
                case CollectionChangeType.Add:
                case CollectionChangeType.RemoveValue:
                case CollectionChangeType.Unspecified:
                case CollectionChangeType.Insert:
                case CollectionChangeType.RemoveKey:
                case CollectionChangeType.SetKey:
                default:
                    break;
            }
        }
        
        private void AfterEnemyChange(CollectionChangeInfo change)
        {
            if (change.ChangeType == CollectionChangeType.Add)
                _turnOrder.Add(new((EntityForEncounterModel)change.Value));
        }

        private void ResetTurnOrder()
        {
            _turnOrder = new();
            _turnOrder.Add(new("Hero"));
            _turnOrder.Add(new("Player"));
        }
#endif

    }

    [Serializable]
    public class EntityForEncounterModel : IEquatable<EntityForEncounterModel>
    {
        public string Id;
        public EntityModel Model;
        public List<SkillModel> SkillOrder;

        public EntityForEncounterModel(string id)
        {
            Id = id;
        }

        public bool Equals(EntityForEncounterModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityForEncounterModel) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }

    [Serializable]
    public class SkillForEncounterModel
    {
        public SkillModel Skill;
        //[ValueDropdown("GetTargets")]
        public string Target = "Hero";

        public SkillForEncounterModel(SkillModel skill)
        {
            Skill = skill;
        }
        
        #if UNITY_EDITOR
        private ValueDropdownList<string> GetTargets(EncounterModel root)
        {
            var res = new ValueDropdownList<string>() {"Hero"};
            foreach (var enemy in root.Enemies)
                res.Add(enemy.Id);
            return res;
        }
        #endif
    }

    [Serializable]
    [InlineProperty, HideLabel]
    public class EntityPointer : IEquatable<EntityPointer>
    {
        [SerializeField, HideInInspector]
        private EntityForEncounterModel _model;

        [HideLabel]
        [ShowInInspector] public string Id => _model.Id;

        public EntityPointer(EntityForEncounterModel model)
        {
            _model = model;
        }
        
        public EntityPointer(string id)
        {
            _model = new(id);
        }

        public bool Equals(EntityPointer other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntityPointer) obj);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }
    }


}