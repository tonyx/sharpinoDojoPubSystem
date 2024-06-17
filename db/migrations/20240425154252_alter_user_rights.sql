-- migrate:up

GRANT ALL ON TABLE public.events_01_kitchen TO safe;
GRANT ALL ON TABLE public.snapshots_01_kitchen TO safe;
GRANT ALL ON SEQUENCE public.snapshots_01_kitchen_id_seq TO safe;
GRANT ALL ON SEQUENCE public.events_01_kitchen_id_seq TO safe;

GRANT ALL ON TABLE public.aggregate_events_01_dishes TO safe;
GRANT ALL ON SEQUENCE public.aggregate_events_01_dishes_id_seq to safe;
GRANT ALL ON TABLE public.events_01_dishes to safe;
GRANT ALL ON TABLE public.snapshots_01_dishes to safe;
GRANT ALL ON SEQUENCE public.snapshots_01_dishes_id_seq to safe;

GRANT ALL ON TABLE public.aggregate_events_01_ingredients TO safe;
GRANT ALL ON SEQUENCE public.aggregate_events_01_ingredients_id_seq to safe;
GRANT ALL ON TABLE public.events_01_ingredients to safe;
GRANT ALL ON TABLE public.snapshots_01_ingredients to safe;
GRANT ALL ON SEQUENCE public.snapshots_01_ingredients_id_seq to safe;

GRANT postgres to safe;    -- dangerous zone!!!! This is to allow at applicative level the classic optimistic lock aggregateState check.


-- migrate:down

