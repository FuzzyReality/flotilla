import { Deck } from 'models/Deck'
import { DeckInspectionTuple, DeckMissionType, Inspection } from './InspectionSection'
import { useLanguageContext } from 'components/Contexts/LanguageContext'
import { CardMissionInformation, StyledDict, compareInspections, getDeadlineInspection } from './InspectionUtilities'
import { Button, Icon, Tooltip, Typography } from '@equinor/eds-core-react'
import { Icons } from 'utils/icons'
import { tokens } from '@equinor/eds-tokens'
import { useMissionsContext } from 'components/Contexts/MissionListsContext'

interface IDeckCardProps {
    deckMissions: DeckMissionType
    setSelectedDeck: React.Dispatch<React.SetStateAction<Deck | undefined>>
    selectedDeck: Deck | undefined
    handleScheduleAll: (inspections: Inspection[]) => void
}

interface DeckCardProps {
    deckData: DeckInspectionTuple
    deckName: string
    setSelectedDeck: React.Dispatch<React.SetStateAction<Deck | undefined>>
    selectedDeck: Deck | undefined
    handleScheduleAll: (inspections: Inspection[]) => void
}

const DeckCard = ({ deckData, deckName, setSelectedDeck, selectedDeck, handleScheduleAll }: DeckCardProps) => {
    const { TranslateText } = useLanguageContext()
    const { ongoingMissions } = useMissionsContext()

    const getCardColor = (deckName: string) => {
        const inspections = deckData.inspections
        if (inspections.length === 0) return 'gray'
        const sortedInspections = inspections.sort(compareInspections)

        if (sortedInspections.length === 0) return 'green'

        const nextInspection = sortedInspections[0]

        if (!nextInspection.deadline) {
            if (!nextInspection.missionDefinition.inspectionFrequency) return 'green'
            else return 'red'
        }

        return getDeadlineInspection(nextInspection.deadline)
    }

    const formattedAreaNames = deckData.areas
        .map((area) => {
            return area.areaName.toLocaleUpperCase()
        })
        .sort()
        .join(' | ')

    return (
        <StyledDict.DeckCard key={deckName}>
            <StyledDict.Rectangle style={{ background: `${getCardColor(deckName)}` }} />
            <StyledDict.Card
                key={deckName}
                onClick={deckData.inspections.length > 0 ? () => setSelectedDeck(deckData.deck) : undefined}
                style={selectedDeck === deckData.deck ? { border: `solid ${getCardColor(deckName)} 2px` } : {}}
            >
                <StyledDict.DeckText>
                    <StyledDict.TopDeckText>
                        <Typography variant={'body_short_bold'}>{deckName.toString()}</Typography>
                        {deckData.inspections
                            .filter((i) => ongoingMissions.find((m) => m.missionId === i.missionDefinition.id))
                            .map((inspection) => (
                                <StyledDict.Content key={inspection.missionDefinition.id}>
                                    <Icon name={Icons.Ongoing} size={16} />
                                    {TranslateText('InProgress')}
                                </StyledDict.Content>
                            ))}
                    </StyledDict.TopDeckText>
                    {deckData.areas && <Typography variant={'body_short'}>{formattedAreaNames}</Typography>}
                    {deckData.inspections && (
                        <CardMissionInformation deckName={deckName} inspections={deckData.inspections} />
                    )}
                </StyledDict.DeckText>
                <StyledDict.CardComponent>
                    <Tooltip
                        placement="top"
                        title={deckData.inspections.length > 0 ? '' : TranslateText('No planned inspection')}
                    >
                        <Button
                            disabled={deckData.inspections.length === 0}
                            variant="outlined"
                            onClick={() => handleScheduleAll(deckData.inspections)}
                            color="secondary"
                        >
                            <Icon name={Icons.LibraryAdd} color={deckData.inspections.length > 0 ? '' : 'grey'} />
                            <Typography color={tokens.colors.text.static_icons__secondary.rgba}>
                                {TranslateText('Queue the missions')}
                            </Typography>
                        </Button>
                    </Tooltip>
                </StyledDict.CardComponent>
            </StyledDict.Card>
        </StyledDict.DeckCard>
    )
}

export function DeckCards({ deckMissions, setSelectedDeck, selectedDeck, handleScheduleAll }: IDeckCardProps) {
    const { TranslateText } = useLanguageContext()

    return (
        <StyledDict.DeckCards>
            {Object.keys(deckMissions).length > 0 ? (
                Object.keys(deckMissions).map((deckName) => (
                    <DeckCard
                        key={'deckCard' + deckName}
                        deckData={deckMissions[deckName]}
                        deckName={deckName}
                        setSelectedDeck={setSelectedDeck}
                        selectedDeck={selectedDeck}
                        handleScheduleAll={handleScheduleAll}
                    />
                ))
            ) : (
                <StyledDict.Placeholder>
                    <Typography variant="h4" color="disabled">
                        {TranslateText('No deck inspections available')}
                    </Typography>
                </StyledDict.Placeholder>
            )}
        </StyledDict.DeckCards>
    )
}
