import { useTRPC } from "@/services/serverFunctions/trpcClient";
import { useSuspenseQuery } from "@tanstack/react-query";

export const directoryKeys = {
  emptyFolders: ["empty folders"] as const,
};

export const useEmptyDirectoriesQuery = () => {
  const trpc = useTRPC();
  return useSuspenseQuery(trpc.directories.getEmptyDirectories.queryOptions());
};
