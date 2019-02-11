#install.packages("rjson")

filterQueries = function(taskBoundariesMatrix, actions, savedQueries) {
  
  resultQueries = c()
  taskId = 1
  
  closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId])- 20000,
                        suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
  resultActions = actions[,actions[1,] >= closestResetAll]
  
  resultTaskBoundaries = list("ClosestResetAll" = closestResetAll, "ServerStart" = taskBoundariesMatrix[1,taskId],"ServerEnd" = taskBoundariesMatrix[2,taskId],
                              "TaskName" = taskBoundariesMatrix[3,taskId], "TaskId" = taskBoundariesMatrix[4,taskId])
  
  for (queryId in 1:length(savedQueries))
  {
    if (savedQueries[queryId] > taskBoundariesMatrix[2,taskId] && taskId < length(taskBoundariesMatrix[1,])) { #after end
      taskId = taskId + 1
      closestResetAll = max(as.numeric(taskBoundariesMatrix[1,taskId]) - 20000,
                            suppressWarnings(max(as.numeric(t(t(actions[,actions[2,] == "ResetAll" & actions[1,] < taskBoundariesMatrix[1,taskId]]))[1,]))))
      resultActions = resultActions[,resultActions[1,] <= taskBoundariesMatrix[2,taskId-1] | resultActions[1,] >= closestResetAll]
      resultTaskBoundaries = rbind(resultTaskBoundaries, list(closestResetAll,taskBoundariesMatrix[1,taskId],taskBoundariesMatrix[2,taskId],
                                                              taskBoundariesMatrix[3,taskId],taskBoundariesMatrix[4,taskId]))
    }
    
    if (savedQueries[queryId] >= taskBoundariesMatrix[1,taskId] || (!is.infinite(closestResetAll) && savedQueries[queryId] > closestResetAll)) { #earlier than start
      if (savedQueries[queryId] <= taskBoundariesMatrix[2,taskId]) 
      {
        resultQueries = cbind(resultQueries, c(savedQueries[queryId], taskBoundariesMatrix[3,taskId]))
      }
    } 
  }
  
  return(list(resultQueries, resultActions, resultTaskBoundaries))
}

isSame = function(firstList, secondList) {
  unlistedFirst = unlist(firstList)
  unlistedSecond = unlist(secondList)
  if (length(unlistedFirst) != length(unlistedSecond)) {
    return(FALSE)
  }
  sum = sum(unlistedFirst != unlistedSecond)
  return(ifelse(sum > 0, FALSE, TRUE))
}

getModelsChanged = function(currentQ, previousQ) {
  
  changes = c()
  for (i in c(1:2)) {
    fs = c("formerquery", "latterquery")[i]
    
    if (!isSame(currentQ[["keywordquery"]][[fs]][["synsetgroups"]], previousQ[["keywordquery"]][[fs]][["synsetgroups"]])) {
      changes = append(changes, paste0("KW_",i))
    }
    if (!isSame(currentQ[["colorsketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["colorsketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Color_",i))
    }
    if (!isSame(currentQ[["facesketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["facesketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Face_",i))
    }
    if (!isSame(currentQ[["textsketchquery"]][[fs]][["colorsketchellipses"]], previousQ[["textsketchquery"]][[fs]][["colorsketchellipses"]])) {
      changes = append(changes, paste0("Text_",i))
    }
    if (!isSame(currentQ[["semanticexamplequery"]][[fs]][["positiveexampleids"]], previousQ[["semanticexamplequery"]][[fs]][["positiveexampleids"]])) {
      changes = append(changes, paste0("Semantic_",i))
    }
    if (!isSame(currentQ[["semanticexamplequery"]][[fs]][["externalimages"]], previousQ[["semanticexamplequery"]][[fs]][["externalimages"]])) {
      changes = append(changes, paste0("External_",i))
    }
  }
  
  return (changes)
}

valueOrEmpty = function(list) {
  return(ifelse(length(list) > 0, list, ""))
}


getEqualKwIds = function(kwLabelRow, keywordLabels) {
  
  kwId = as.character(kwLabelRow[[1]])
  result = c()
  if (kwId != "H") {
    result = c(kwId)
  }
  equalities = unlist(strsplit(as.character(kwLabelRow[[4]]), "#"))
  for (equality in equalities) {
    
    newKwLabelRow = keywordLabels[keywordLabels[,2] == equality,]
    result = append(result, getEqualKwIds(newKwLabelRow, keywordLabels))
  }
  
  return (result)
}


getKwText = function(keywordGroups, keywordLabels) {
  
  parts = c()
  for (synsetGroup in keywordGroups) {
    
    groupIds = sapply(synsetGroup[["synsets"]], function(s) { s[["synsetid"]]})
    localParts = c()
    while(length(groupIds) > 0) {
     
      kwLabelRow = keywordLabels[keywordLabels[,1] == groupIds[1],]
      localParts = append(localParts, as.character(sub("#.*", "", kwLabelRow[[3]])))
      groupIds = groupIds[groupIds %ni% getEqualKwIds(kwLabelRow, keywordLabels)]
    }
    
    parts = append(parts, paste(localParts, collapse = " OR "))
  }
  
  return(paste(parts, collapse = " AND "))
}



library("rjson")
"%ni%" = Negate("%in%")

baseDir = "d:\\Temp\\Downloads\\database-vbs2019\\"

#read valid tasks
competitionsFile = file(paste0(baseDir, "competitions.db"),open="r")
validTasks = fromJSON(readLines(competitionsFile)[2])[["taskSequence"]]
close(competitionsFile)

taskFile = file(paste0(baseDir, "tasks.db"),open="r")
taskLines = readLines(taskFile)
close(taskFile)
validTasks = validTasks[validTasks %in% sapply(taskLines, function(line) {
  jsonData = fromJSON(line)
  ifelse(is.element(jsonData[["_id"]], validTasks) && !startsWith(jsonData[["type"]], "AVS"), jsonData[["_id"]], "")
})]

actionsFile = file(paste0(baseDir, "actionLogs.db"),open="r")
actionLines = readLines(actionsFile)
close(actionsFile)

submissionsFile = file(paste0(baseDir, "submissions.db"),open="r")
submissionsLines = readLines(submissionsFile)
close(submissionsFile)


#tasks definitions
taskBoundariesMatrix = c()
for (line in taskLines)
{
  jsonData = fromJSON(line)
  if (!is.element(jsonData[["_id"]], validTasks)) {
    next
  }
  taskBoundariesMatrix = cbind(taskBoundariesMatrix, c(jsonData[["startTimeStamp"]], jsonData[["endTimeStamp"]], jsonData[["name"]], jsonData[["_id"]]))
}

taskBoundariesMatrix = taskBoundariesMatrix[,order(taskBoundariesMatrix[1,])]
taskBoundaries = as.vector(taskBoundariesMatrix)

for (member in c(0,1)) {
  
  #actions
  actions = c()
  allActions = c()
  for (line in actionLines)
  {
    actionData = fromJSON(line)
    if (is.null(actionData[["teamId"]]) || actionData[["teamId"]] != 4 || actionData[["memberId"]] != member || length(actionData[["events"]]) <= 0) {
      next
    }
      
    allActions = cbind(allActions, sapply(actionData[["events"]], 
                                          function(event) { c(ifelse(is.null(event[["timestamp"]]), NA, event[["timestamp"]]),
                                                              ifelse(is.null(event[["type"]]), NA, event[["type"]]),
                                                              ifelse(is.null(event[["category"]]), NA, event[["category"]]),
                                                              ifelse(is.null(event[["value"]]), NA, event[["value"]])) }))
    if (is.element(actionData[["taskId"]], validTasks)) {
      actions = cbind(actions, sapply(actionData[["events"]], 
                                      function(event) { c(event[["timestamp"]],event[["type"]],event[["category"]],
                                      ifelse(is.null(event[["value"]]), "NA", event[["value"]])) }))
    }
  }
  
  #submissions
  submissions = c()
  for (line in submissionsLines)
  {
    subData = fromJSON(line)
    if (is.null(subData[["teamNumber"]]) || subData[["teamNumber"]] != 4 || subData[["memberNumber"]] != member
        || !is.element(subData[["taskId"]], validTasks)
    ) {
      next
    }
    
    submissions = cbind(submissions, c(subData[["timestamp"]], ifelse(is.null(subData[["correct"]]), 0, subData[["correct"]]),
                                       subData[["taskId"]]))
  }
  
  #saved tasks
  baseSavedQPath = "..\\..\\..\\Logs\\2019-01-09_VBS2019\\"
  allSavedQueries = as.numeric(sub(".json$", "", list.files(paste0(baseSavedQPath, ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"), "\\QueriesLog_sortingModelsUpdated"))))
  
  newQueriesAndActions = filterQueries(taskBoundariesMatrix, actions, allSavedQueries)
  filteredSavedQueries = newQueriesAndActions[[1]]
  actions = newQueriesAndActions[[2]]
  filteredTaskBoundaries = newQueriesAndActions[[3]]
  
  write.table(filteredTaskBoundaries, paste0("TaskBoundaries_member_",member,".csv"), row.names = FALSE, sep = ";")
  write.table(t(submissions), paste0("Submissions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  write.table(t(actions[,actions[3,] == "Browsing"]), paste0("Actions_member_",member,".csv"), col.names = FALSE, row.names = FALSE, sep = ";")
  #task order
  write.table(sapply(validTasks, function(taskId) {filteredTaskBoundaries[filteredTaskBoundaries[,"TaskId"]==taskId,][["TaskName"]]}),
              paste0("Tasks_order_member_",member,".csv"), col.names = TRUE, row.names = FALSE, sep = ";")
  
  queryResultsFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),".txt"),open="r")
  queryResults = unname(sapply(readLines(queryResultsFile),
                        function(line) {
                          queryResJson = fromJSON(line)
                          return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                        }))
  close(queryResultsFile)
  
  queryResultsBeforeFilterFile = file(paste0("ResultRankings_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"_BeforeCountFilter.txt"),open="r")
  queryResultsBF = unname(sapply(readLines(queryResultsBeforeFilterFile),
                               function(line) {
                                 queryResJson = fromJSON(line)
                                 return (t(c(queryResJson[["queryTimestamp"]], queryResJson[["topVideoPosition"]],queryResJson[["topShotPosition"]])))
                               }))
  close(queryResultsBeforeFilterFile)
  
  
  timestamps = read.csv(paste0("timestamps_",ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),".csv"), header = TRUE, sep = ";")
  taskNames = read.csv("tasknames.csv", header = TRUE, sep = ";")
  keywordLabels = read.csv("V3C1-GoogLeNet.label", header = FALSE, sep = "~")
  
  #transform queries
  transformedQueries = c()
  allSavedQueries = allSavedQueries[order(allSavedQueries)]
  previousSimilarity = NULL
  for (queryTS in allSavedQueries) {
    
    queryFile = file(paste0(baseSavedQPath,ifelse(member == 0,"SIRIUS-PC","PREMEK-NTB"),"\\QueriesLog_sortingModelsUpdated\\",queryTS,".json"),open="r")
    queryData = fromJSON(file = queryFile)
    close(queryFile)
    
    mainModel = queryData[["primarytemporalquery"]]
    firstIsMain = mainModel == 0
    first = ifelse(firstIsMain, "formerquery", "latterquery")
    second = ifelse(!firstIsMain, "formerquery", "latterquery")
    similarity = queryData[["bitemporalsimilarityquery"]]
    
    if (!is.element(queryTS, filteredSavedQueries[1,])) {
      previousSimilarity = similarity
      next
    }
    
    taskName = filteredSavedQueries[2,filteredSavedQueries[1,] == queryTS]
    taskId = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["TaskId"]]
    firstSucSub = min(submissions[,submissions[3,]==taskId & submissions[2,]=="TRUE"][1])
    fileTimes = timestamps[timestamps[,"Filename"] == paste0(queryTS,".json"),]
    transformedQuery = list(
      "TimeStamp" = queryTS,
      "TaskName" = taskName,
      "TaskNameShort" = toString(taskNames[taskNames[,"TaskName"] == taskName,"TaskID"]),
      "TopVideoPosition" = queryResults[,queryResults[1,] == queryTS][2],
      "TopShotPosition" = queryResults[,queryResults[1,] == queryTS][3],
      "TopVideoPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][2],
      "TopShotPositionBeforeFilter" = queryResultsBF[,queryResultsBF[1,] == queryTS][3],
      "KW_1_Count" = length(similarity[["keywordquery"]][[first]][["synsetgroups"]]),
      "KW_1_Words" = getKwText(similarity[["keywordquery"]][[first]][["synsetgroups"]],keywordLabels ),
      "KW_1_IDS" = paste(sapply(similarity[["keywordquery"]][[first]][["synsetgroups"]], 
                          function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "KW_2_Count" = length(similarity[["keywordquery"]][[second]][["synsetgroups"]]),
      "KW_2_Words" = getKwText(similarity[["keywordquery"]][[second]][["synsetgroups"]], keywordLabels),
      "KW_2_IDS" = paste(sapply(similarity[["keywordquery"]][[second]][["synsetgroups"]], 
                                function(g) { paste(sapply(g[["synsets"]], function(s) { s[["synsetid"]]}),collapse ="+")}),collapse ="|"),
      "Color_1" = length(similarity[["colorsketchquery"]][[first]][["colorsketchellipses"]]),
      "Color_2" = length(similarity[["colorsketchquery"]][[second]][["colorsketchellipses"]]),
      "Face_1" = length(similarity[["facesketchquery"]][[first]][["colorsketchellipses"]]),
      "Face_2" = length(similarity[["facesketchquery"]][[second]][["colorsketchellipses"]]),
      "Text_1" = length(similarity[["textsketchquery"]][[first]][["colorsketchellipses"]]),
      "Text_2" = length(similarity[["textsketchquery"]][[second]][["colorsketchellipses"]]),
      "Semantic_1" = ifelse(length(similarity[["semanticexamplequery"]][[first]][["positiveexampleids"]]) > 0, "on",
                            ifelse(length(similarity[["semanticexamplequery"]][[first]][["externalimages"]]) > 0, "external", "off")),
      "Semantic_2" = ifelse(length(similarity[["semanticexamplequery"]][[second]][["positiveexampleids"]]) > 0, "on", 
                            ifelse(length(similarity[["semanticexamplequery"]][[second]][["externalimages"]]) > 0, "external", "off")),
      "SortedBy" = switch(as.numeric(queryData[[ifelse(queryData[["primarytemporalquery"]] == 0, "formerfusionquery", "latterfusionquery")]][["sortingsimilaritymodel"]]) + 1,
                          "Keyword", "ColorSketch", "Face", "Text", "Semantic", "None"),
      "ModelsChanged" = paste(getModelsChanged(similarity, previousSimilarity), collapse = "|"),
      "TaskStart" = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["ServerStart"]],
      "TaskEnd" = filteredTaskBoundaries[filteredTaskBoundaries[,"TaskName"]==taskName,][["ServerEnd"]],
      "FirstSuccessfulSubmission" = firstSucSub,
      "IsAfterSuccessfulSubmission" = queryTS > firstSucSub,
      "QueryFileCreated" = valueOrEmpty(fileTimes[["CreatedUnixMiliseconds"]]),
      "QueryFileModified" = valueOrEmpty(fileTimes[["ModifiedUnixMiliseconds"]]),
      "QueryFileModifiedDiff" = valueOrEmpty(fileTimes[["CreatedModifiedDifferenceSeconds"]])
    )
    
    previousSimilarity = similarity
    transformedQueries = rbind(transformedQueries, transformedQuery)
  }
  write.table(transformedQueries, paste0("Queries_member_",member,".csv"), row.names = FALSE, sep = ";")
}





