namespace AFMS.Models
{
    /// <summary>
    /// Generic Binary Search Tree implementation for Flight data
    /// Provides O(log n) average case search, insert, and delete operations
    /// </summary>
    public class FlightBinarySearchTree
    {
        private class Node
        {
            public Flight Flight { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public Node(Flight flight)
            {
                Flight = flight;
                Left = null;
                Right = null;
            }
        }

        private Node root;
        private int count;

        public FlightBinarySearchTree()
        {
            root = null;
            count = 0;
        }

        /// <summary>
        /// Inserts a flight into the BST
        /// Time Complexity: O(log n) average, O(n) worst case
        /// </summary>
        public void Insert(Flight flight)
        {
            root = InsertRec(root, flight);
            count++;
        }

        private Node InsertRec(Node node, Flight flight)
        {
            if (node == null)
                return new Node(flight);

            if (string.Compare(flight.FlightNumber, node.Flight.FlightNumber) < 0)
                node.Left = InsertRec(node.Left, flight);
            else if (string.Compare(flight.FlightNumber, node.Flight.FlightNumber) > 0)
                node.Right = InsertRec(node.Right, flight);

            return node;
        }

        /// <summary>
        /// Searches for a flight by flight number
        /// Time Complexity: O(log n) average, O(n) worst case
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
        /// Deletes a flight from the BST
        /// Time Complexity: O(log n) average, O(n) worst case
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
        /// Time Complexity: O(n)
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
    }
}
