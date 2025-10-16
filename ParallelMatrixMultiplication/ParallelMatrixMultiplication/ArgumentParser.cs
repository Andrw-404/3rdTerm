// <copyright file="ArgumentParser.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication
{
    /// <summary>
    /// A class for working with command line arguments.
    /// </summary>
    public static class ArgumentParser
    {
        /// <summary>
        /// Analyzes the command line arguments and determines the execution mode.
        /// </summary>
        /// <param name="args">An array of command line arguments.</param>
        /// <returns>A tuple containing the execution mode and an array of parameters.</returns>
        public static (string Mode, string[] Parametrs) Parse(string[] args)
        {
            string mode = args[0].ToLower();
            string[] parametrs = args.Skip(1).ToArray();
            return (mode, parametrs);
        }
    }
}
