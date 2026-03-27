namespace AFMS.Models
{
    /// <summary>
    /// AVL Tree (Self-Balancing BST) implementation for Flight data
    /// Maintains balance to ensure O(log n) for all operations
    /// Time Complexity: Insert O(log n), Search O(log n), Delete O(log n)
    /// </summary>
    public class FlightAVLTree
    {
        private class Node
        {
            public Flight Flight { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
            public int Height { get; set; }

            public Node(Flight flight)
            {
                Flight = flight;
                Left = null;
                Right = null;
                Height = 1;
            }
        }

        private Node root;
        private int count;

        public FlightAVLTree()
        {
            root = null;
            count = 0;
        }

        private int GetHeight(Node node) => node == null ? 0 : node.Height;

        private int GetBalance(Node node) => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

        private Node RotateRight(Node y)
        {
            Node x = y.Left;
            Node t2 = x.Right;

            x.Right = y;
            y.Left = t2;

            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));
            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));

            return x;
        }

        private Node RotateLeft(Node x)
        {
            Node y = x.Right;
            Node t2 = y.Left;

            y.Left = x;
            x.Right = t2;

            x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));
            y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));

            return y;
        }

        /// <summary>
        /// Inserts a flight into the AVL tree with automatic balancing
        /// </summary>
        public void Insert(Flight flight)
        {
            root = InsertRec(root, flight);
        }

        private Node InsertRec(Node node, Flight flight)
        {
            if (node == null)
            {
                count++;
                return new Node(flight);
            }

            int cmp = string.Compare(flight.FlightNumber, node.Flight.FlightNumber);
            if (cmp < 0)
                node.Left = InsertRec(node.Left, flight);
            else if (cmp > 0)
                node.Right = InsertRec(node.Right, flight);
            else
                return node;

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            int balance = GetBalance(node);

            if (balance > 1 && string.Compare(flight.FlightNumber, node.Left.Flight.FlightNumber) < 0)
                return RotateRight(node);

            if (balance < -1 && string.Compare(flight.FlightNumber, node.Right.Flight.FlightNumber) > 0)
                return RotateLeft(node);

            if (balance > 1 && string.Compare(flight.FlightNumber, node.Left.Flight.FlightNumber) > 0)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            if (balance < -1 && string.Compare(flight.FlightNumber, node.Right.Flight.FlightNumber) < 0)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        /// <summary>
        /// Searches for a flight by flight number
        /// </summary>
        public Flight Search(string flightNumber)
        {
            return SearchRec(root, flightNumber);
        }

        private Flight SearchRec(Node node, string flightNumber)
        {
            if (node == null)
                return null;

            int cmp = string.Compare(flightNumber, node.Flight.FlightNumber);
            if (cmp < 0)
                return SearchRec(node.Left, flightNumber);
            else if (cmp > 0)
                return SearchRec(node.Right, flightNumber);
            else
                return node.Flight;
        }

        /// <summary>
        /// Deletes a flight from the AVL tree with rebalancing
        /// </summary>
        public bool Delete(string flightNumber)
        {
            int oldCount = count;
            root = DeleteRec(root, flightNumber);
            return count < oldCount;
        }

        private Node DeleteRec(Node node, string flightNumber)
        {
            if (node == null)
                return null;

            int cmp = string.Compare(flightNumber, node.Flight.FlightNumber);
            if (cmp < 0)
                node.Left = DeleteRec(node.Left, flightNumber);
            else if (cmp > 0)
                node.Right = DeleteRec(node.Right, flightNumber);
            else
            {
                count--;
                if (node.Left == null)
                    return node.Right;
                else if (node.Right == null)
                    return node.Left;

                Node minRight = FindMin(node.Right);
                node.Flight = minRight.Flight;
                node.Right = DeleteRec(node.Right, minRight.Flight.FlightNumber);
            }

            if (node == null)
                return null;

            node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            int balance = GetBalance(node);

            if (balance > 1 && GetBalance(node.Left) >= 0)
                return RotateRight(node);

            if (balance > 1 && GetBalance(node.Left) < 0)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            if (balance < -1 && GetBalance(node.Right) <= 0)
                return RotateLeft(node);

            if (balance < -1 && GetBalance(node.Right) > 0)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            return node;
        }

        private Node FindMin(Node node)
        {
            while (node.Left != null)
                node = node.Left;
            return node;
        }

        /// <summary>
        /// Retrieves all flights in sorted order
        /// </summary>
        public List<Flight> GetAllFlightsSorted()
        {
            List<Flight> flights = new List<Flight>();
            InorderTraversal(root, flights);
            return flights;
        }

        private void InorderTraversal(Node node, List<Flight> flights)
        {
            if (node == null)
                return;

            InorderTraversal(node.Left, flights);
            flights.Add(node.Flight);
            InorderTraversal(node.Right, flights);
        }

        public int Count => count;
        public int Height => GetHeight(root);
    }
}
