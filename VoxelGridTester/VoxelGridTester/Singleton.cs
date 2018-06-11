/// Singleton
/// Custom written source for singleton pattern... could not find prewritten code to use.
/// Mark Scherer, June 2018

/// <summary>
/// Source for singleton pattern... only allows one instance of type T in global namespace.
/// NOTE: Enforces new constraint on T: T can only have single, parameter-less constructor... 
    /// i.e. return of new T() must be only desired configuration of T.
/// </summary>
public class Singleton<T> where T : new()
{
    /// <summary>
    /// Single instance of class T.
    /// </summary>
    private static T myInstance;

    /// <summary>
    /// Lock protecting myInstance
    /// </summary>
    private static object myLock = new object();

    /// <summary>
    /// Accessor for myInstance.
    /// DO NOT USE T's constructor... ALWAYS use Instance !
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (myLock)
            {
                if (myInstance == null)
                    myInstance = new T();
                return myInstance;
            }
        }
    }
}