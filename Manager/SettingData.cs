
namespace ShoulderSurfing
{

    public abstract class AbstractSettingObject
    {
        public abstract string GetName();
        public abstract object GetValue();
        public abstract void Load();
        public abstract void Register();
    }

    // 泛型设置对象
    public class SettingObject<T> : AbstractSettingObject
    {
        public string name;
        public string descCN;
        public string descEN;
        public Action<SettingObject<T>> loadFunc;
        public Action<SettingObject<T>> registerFunc;
        public Action<T> valueChangeFunc;
        public Func<T> getValueFunc;

        public SettingObject<T> SetName(string name)
        {
            this.name = name;
            return this;
        }

        public SettingObject<T> SetValueChangeFunc(Action<T> valueChangeFunc)
        {
            this.valueChangeFunc = valueChangeFunc;
            return this;
        }
        public SettingObject<T> SetDescCN(string descCN)
        {
            this.descCN = descCN;
            return this;
        }

        public SettingObject<T> SetDescEN(string descEN)
        {
            this.descEN = descEN;
            return this;
        }

        public SettingObject<T> SetLoadFunc(Action<SettingObject<T>> loadFunc)
        {
            this.loadFunc = loadFunc;
            return this;
        }

        public SettingObject<T> SetRegisterFunc(Action<SettingObject<T>> registerFunc)
        {
            this.registerFunc = registerFunc;
            return this;
        }

        public SettingObject<T> SetGetValueFunc(Func<T> getValueFunc)
        {
            this.getValueFunc = getValueFunc;
            return this;
        }

        // 实现抽象方法
        public override string GetName() => name;

        public override object GetValue()
        {
            return getValueFunc != null ? getValueFunc() : default(T);
        }

        public override void Load()
        {
            loadFunc?.Invoke(this);
        }

        public override void Register()
        {
            registerFunc?.Invoke(this);
        }

        // 强类型获取值
        public T GetTypedValue()
        {
            return getValueFunc != null ? getValueFunc() : default(T);
        }
    }
}