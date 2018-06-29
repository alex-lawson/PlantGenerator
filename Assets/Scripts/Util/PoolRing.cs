public class PoolRing<T> where T : new() {

    private T[] ring;
    private int size;
    private int index;

    public PoolRing(int size) {
        ring = new T[size];
        index = -1;
        this.size = size;

        for (int i = 0; i < size; i++)
            ring[i] = new T();
    }

    public T GetNext() {
        index = (index + 1) % size;
        return ring[index];
    }
}
